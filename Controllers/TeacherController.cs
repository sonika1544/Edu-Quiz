using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduQuiz_.Data;
using EduQuiz_.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace EduQuiz_.Controllers
{
    [Authorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeacherController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var subjects = await _context.TeacherSubjects
                .Include(ts => ts.Subject)
                .Where(ts => ts.TeacherId == userId && ts.IsActive)
                .ToListAsync();

            return View(subjects);
        }

        public async Task<IActionResult> Units(int? subjectId)
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var teacherSubjects = await _context.TeacherSubjects
                .Include(ts => ts.Subject)
                .Where(ts => ts.TeacherId == userId && ts.IsActive)
                .ToListAsync();

            if (!subjectId.HasValue)
            {
                var firstSubject = teacherSubjects.FirstOrDefault();
                if (firstSubject == null)
                {
                    ViewBag.SubjectId = null;
                    ViewBag.Subjects = teacherSubjects;
                    return View(new List<Unit>());
                }
                subjectId = firstSubject.SubjectId;
            }

            var units = await _context.Units
                .Where(u => u.SubjectId == subjectId.Value)
                .OrderBy(u => u.UnitNumber)
                .ToListAsync();

            ViewBag.SubjectId = subjectId.Value;
            ViewBag.Subjects = teacherSubjects;
            return View(units);
        }

        public IActionResult AddUnit(int subjectId)
        {
            ViewBag.SubjectId = subjectId;
            var unit = new Unit { SubjectId = subjectId };
            return View(unit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUnit(Unit unit)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (unit.UnitNumber < 1)
                    {
                        var maxUnitNumber = await _context.Units
                            .Where(u => u.SubjectId == unit.SubjectId)
                            .MaxAsync(u => (int?)u.UnitNumber) ?? 0;
                        unit.UnitNumber = maxUnitNumber + 1;
                    }

                    _context.Units.Add(unit);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Units", new { subjectId = unit.SubjectId });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception saving unit: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the unit. Please try again.");
                }
            }
            else
            {
                foreach (var modelStateKey in ModelState.Keys)
                {
                    var modelStateVal = ModelState[modelStateKey];
                    foreach (var error in modelStateVal.Errors)
                    {
                        System.Diagnostics.Debug.WriteLine($"ModelState error on {modelStateKey}: {error.ErrorMessage}");
                    }
                }
            }
            ViewBag.SubjectId = unit.SubjectId;
            return View(unit);
        }

        // Added ListQuestions action
        public async Task<IActionResult> ListQuestions(int quizId)
        {
            System.Diagnostics.Debug.WriteLine($"ListQuestions called with quizId: {quizId}");

            var questions = await _context.Questions
                .Include(q => q.Quiz)
                .Where(q => q.QuizId == quizId)
                .ToListAsync();

            System.Diagnostics.Debug.WriteLine($"Number of questions found: {questions.Count}");

            return View("QuestionsList", questions);
        }

        public async Task<IActionResult> ManageQuestions(int quizId)
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var quiz = await _context.Quizzes
                .Include(q => q.Unit)
                .ThenInclude(u => u.Subject)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null)
            {
                return NotFound();
            }

            // Verify that the teacher has access to this quiz's subject
            var hasAccess = await _context.TeacherSubjects
                .AnyAsync(ts => ts.TeacherId == userId && 
                               ts.SubjectId == quiz.Unit.SubjectId && 
                               ts.IsActive);

            if (!hasAccess)
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            var questions = await _context.Questions
                .Where(q => q.QuizId == quizId)
                .ToListAsync();

            ViewBag.QuizId = quizId;
            return View(questions);
        }

        // POST: Teacher/LaunchQuiz/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LaunchQuiz(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }

            if (quiz.StartTime != null)
            {
                // Quiz already started, optionally handle this case
                return RedirectToAction("Quizzes");
            }

            quiz.StartTime = DateTime.Now;
            quiz.EndTime = quiz.StartTime?.AddMinutes(quiz.DurationMinutes);

            _context.Quizzes.Update(quiz);
            await _context.SaveChangesAsync();

            return RedirectToAction("Quizzes");
        }

        public async Task<IActionResult> EditUnit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var unit = await _context.Units.FindAsync(id);
            if (unit == null)
            {
                return NotFound();
            }
            ViewBag.SubjectId = unit.SubjectId;
            return View(unit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUnit(int id, Unit unit)
        {
            if (id != unit.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(unit);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Units", new { subjectId = unit.SubjectId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Units.Any(e => e.Id == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ViewBag.SubjectId = unit.SubjectId;
            return View(unit);
        }

        public async Task<IActionResult> DeleteUnit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var unit = await _context.Units
                .FirstOrDefaultAsync(m => m.Id == id);
            if (unit == null)
            {
                return NotFound();
            }

            ViewBag.SubjectId = unit.SubjectId;
            return View(unit);
        }

        [HttpPost, ActionName("DeleteUnit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUnitConfirmed(int id)
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit != null)
            {
                _context.Units.Remove(unit);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Units", new { subjectId = unit.SubjectId });
        }

        public async Task<IActionResult> Quizzes(int? subjectId)
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var teacherSubjects = await _context.TeacherSubjects
                .Include(ts => ts.Subject)
                .Where(ts => ts.TeacherId == userId && ts.IsActive)
                .ToListAsync();

            if (!subjectId.HasValue)
            {
                var firstSubject = teacherSubjects.FirstOrDefault();
                if (firstSubject == null)
                {
                    ViewBag.SubjectId = null;
                    ViewBag.Subjects = teacherSubjects;
                    return View(new List<Quiz>());
                }
                subjectId = firstSubject.SubjectId;
            }

            var quizzes = await _context.Quizzes
                .Include(q => q.Unit)
                .Where(q => q.Unit != null && q.Unit.SubjectId == subjectId.Value)
                .OrderBy(q => q.Title)
                .ToListAsync();

            ViewBag.SubjectId = subjectId.Value;
            ViewBag.Subjects = teacherSubjects;
            return View(quizzes);
        }

        public async Task<IActionResult> AddQuiz()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                return RedirectToAction("Login", "Account");
            }
            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var teacherSubjects = await _context.TeacherSubjects
                .Include(ts => ts.Subject)
                .Where(ts => ts.TeacherId == userId && ts.IsActive)
                .ToListAsync();

            var subjectIds = teacherSubjects.Select(ts => ts.SubjectId).ToList();

            var units = await _context.Units
                .Where(u => subjectIds.Contains(u.SubjectId))
                .OrderBy(u => u.Name)
                .ToListAsync();

            ViewBag.Units = units;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuiz(Quiz quiz)
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                return RedirectToAction("Login", "Account");
            }
            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Do not set StartTime and EndTime here; will be set on launch
                    quiz.StartTime = null;
                    quiz.EndTime = null;

                    _context.Quizzes.Add(quiz);
                    await _context.SaveChangesAsync();

                    // Get the subjectId of the newly added quiz's unit
                    var unit = await _context.Units.FindAsync(quiz.UnitId);
                    int? subjectId = unit?.SubjectId;

                    return RedirectToAction("Quizzes", new { subjectId = subjectId });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception saving quiz: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the quiz. Please try again.");
                }
            }

            var teacherSubjects = await _context.TeacherSubjects
                .Include(ts => ts.Subject)
                .Where(ts => ts.TeacherId == userId && ts.IsActive)
                .ToListAsync();

            var subjectIds = teacherSubjects.Select(ts => ts.SubjectId).ToList();

            var units = await _context.Units
                .Where(u => subjectIds.Contains(u.SubjectId))
                .OrderBy(u => u.Name)
                .ToListAsync();

            ViewBag.Units = units;

            return View(quiz);
        }

        // GET: Teacher/EditQuiz/5
        public async Task<IActionResult> EditQuiz(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                return RedirectToAction("Login", "Account");
            }
            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var teacherSubjects = await _context.TeacherSubjects
                .Include(ts => ts.Subject)
                .Where(ts => ts.TeacherId == userId && ts.IsActive)
                .ToListAsync();

            var subjectIds = teacherSubjects.Select(ts => ts.SubjectId).ToList();

            var units = await _context.Units
                .Where(u => subjectIds.Contains(u.SubjectId))
                .OrderBy(u => u.Name)
                .ToListAsync();

            ViewBag.Units = units;

            return View(quiz);
        }

        // POST: Teacher/EditQuiz/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditQuiz(int id, Quiz quiz)
        {
            if (id != quiz.Id)
            {
                return NotFound();
            }

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                return RedirectToAction("Login", "Account");
            }
            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(quiz);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Quizzes");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Quizzes.Any(e => e.Id == quiz.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception updating quiz: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the quiz. Please try again.");
                }
            }

            var teacherSubjects = await _context.TeacherSubjects
                .Include(ts => ts.Subject)
                .Where(ts => ts.TeacherId == userId && ts.IsActive)
                .ToListAsync();

            var subjectIds = teacherSubjects.Select(ts => ts.SubjectId).ToList();

            var units = await _context.Units
                .Where(u => subjectIds.Contains(u.SubjectId))
                .OrderBy(u => u.Name)
                .ToListAsync();

            ViewBag.Units = units;

            return View(quiz);
        }

        // GET: Teacher/DeleteQuiz/5
        public async Task<IActionResult> DeleteQuiz(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var quiz = await _context.Quizzes
                .Include(q => q.Unit)
                .ThenInclude(u => u.Subject)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (quiz == null)
            {
                return NotFound();
            }

            return View(quiz);
        }

        // POST: Teacher/DeleteQuiz/5
        [HttpPost, ActionName("DeleteQuizConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteQuizConfirmed(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz != null)
            {
                _context.Quizzes.Remove(quiz);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Quizzes");
        }

        // GET: Teacher/AddQuestion
        public async Task<IActionResult> AddQuestion()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                return RedirectToAction("Login", "Account");
            }
            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Get quizzes created by the teacher
            var teacherSubjects = await _context.TeacherSubjects
                .Where(ts => ts.TeacherId == userId && ts.IsActive)
                .Select(ts => ts.SubjectId)
                .ToListAsync();

            var quizzes = await _context.Quizzes
                .Include(q => q.Unit)
                .ThenInclude(u => u.Subject)
                .Where(q => teacherSubjects.Contains(q.Unit.SubjectId))
                .ToListAsync();

            ViewBag.Quizzes = quizzes;

            return View();
        }

        // POST: Teacher/AddQuestion
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddQuestion(Question question)
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                return RedirectToAction("Login", "Account");
            }
            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Ensure QuizId is set correctly
                    if (question.QuizId == 0)
                    {
                        var quizIdStr = Request.Form["QuizId"].ToString();
                        if (int.TryParse(quizIdStr, out int quizId))
                        {
                            question.QuizId = quizId;
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"Saving question with QuizId: {question.QuizId}, QuestionText: {question.QuestionText}");

                    _context.Questions.Add(question);
                    await _context.SaveChangesAsync();

                    System.Diagnostics.Debug.WriteLine($"Question saved with Id: {question.Id}");

                    return RedirectToAction("ListQuestions", new { quizId = question.QuizId });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Exception saving question: {ex.Message}");
                    ModelState.AddModelError(string.Empty, "An error occurred while saving the question. Please try again.");
                }
            }

            // Reload quizzes for the form if validation fails
            var teacherSubjects = await _context.TeacherSubjects
                .Where(ts => ts.TeacherId == userId && ts.IsActive)
                .Select(ts => ts.SubjectId)
                .ToListAsync();

            var quizzes = await _context.Quizzes
                .Include(q => q.Unit)
                .ThenInclude(u => u.Subject)
                .Where(q => teacherSubjects.Contains(q.Unit.SubjectId))
                .ToListAsync();

            ViewBag.Quizzes = quizzes;

            return View(question);
        }

        public async Task<IActionResult> Materials()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var teacherSubjects = await _context.TeacherSubjects
                .Where(ts => ts.TeacherId == userId && ts.IsActive)
                .Select(ts => ts.SubjectId)
                .ToListAsync();

            var materials = await _context.Materials
                .Include(m => m.Subject)
                .Where(m => teacherSubjects.Contains(m.SubjectId))
                .ToListAsync();

            return View(materials);
        }

        public async Task<IActionResult> AddMaterial()
        {
            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim == null || string.IsNullOrEmpty(userIdClaim.Value))
            {
                return RedirectToAction("Login", "Account");
            }

            if (!int.TryParse(userIdClaim.Value, out var userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var subjects = await _context.TeacherSubjects
                .Include(ts => ts.Subject)
                .Where(ts => ts.TeacherId == userId && ts.IsActive)
                .Select(ts => ts.Subject)
                .ToListAsync();

            ViewBag.Subjects = subjects;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMaterial(Material material, IFormFile file)
        {
            if (ModelState.IsValid && file != null)
            {
                try
                {
                    // Save the file
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    material.FilePath = "/uploads/" + uniqueFileName;
                    material.UploadDate = DateTime.Now;

                    _context.Materials.Add(material);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Materials));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred while uploading the material. Please try again.");
                }
            }

            var userIdClaim = User.FindFirst("UserId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                var subjects = await _context.TeacherSubjects
                    .Include(ts => ts.Subject)
                    .Where(ts => ts.TeacherId == userId && ts.IsActive)
                    .Select(ts => ts.Subject)
                    .ToListAsync();

                ViewBag.Subjects = subjects;
            }

            return View(material);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMaterial(int id)
        {
            var material = await _context.Materials.FindAsync(id);
            if (material == null)
            {
                return NotFound();
            }

            try
            {
                // Delete the file
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", material.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _context.Materials.Remove(material);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Materials));
            }

            return RedirectToAction(nameof(Materials));
        }
    }
}
