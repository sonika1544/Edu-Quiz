using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduQuiz_.Data;
using EduQuiz_.Models;
using EduQuiz_.Services;
using System.Security.Cryptography;
using System.Text;

namespace EduQuiz_.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AdminController(ApplicationDbContext context, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var stats = new
            {
                Teachers = await _context.Teachers.CountAsync(),
                Students = await _context.Users.CountAsync(u => u.Role == UserRole.Student),
                Subjects = await _context.Subjects.CountAsync()
            };

            return View(stats);
        }

        // Teacher Management
        public async Task<IActionResult> Teachers()
        {
            var teachers = await _context.Teachers
                .Include(t => t.TeacherSubjects)
                .ThenInclude(ts => ts.Subject)
                .OrderBy(t => t.FirstName)
                .ToListAsync();
            return View(teachers);
        }

        public IActionResult AddTeacher()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTeacher(Teacher teacher)
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (await _context.Teachers.AnyAsync(t => t.Email == teacher.Email))
                {
                    ModelState.AddModelError("Email", "Email already exists");
                    return View(teacher);
                }

                // Generate a unique token for password setup
                var token = GeneratePasswordSetupToken();
                
                // Set the password reset token and expiry
                teacher.PasswordResetToken = token;
                teacher.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24);
                teacher.CreatedAt = DateTime.UtcNow;
                teacher.IsActive = false; // Teacher will be activated after setting password

                _context.Add(teacher);
                await _context.SaveChangesAsync();

                // Generate password setup URL - Fix the URL format
                var baseUrl = _configuration["ApplicationUrl"] ?? Request.Scheme + "://" + Request.Host;
                var setupUrl = $"{baseUrl}/Account/SetPassword?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(teacher.Email)}";

                try
                {
                    // Send password setup email
                    await _emailService.SendPasswordSetupEmailAsync(
                        teacher.Email,
                        teacher.FirstName,
                        setupUrl
                    );

                    TempData["SuccessMessage"] = "Teacher added successfully. A password setup email has been sent.";
                }
                catch (Exception ex)
                {
                    // Log the error
                    Console.WriteLine($"Error sending email: {ex.Message}");
                    
                    // Still show success but with a warning about email
                    TempData["SuccessMessage"] = "Teacher added successfully.";
                    TempData["WarningMessage"] = "Could not send the password setup email. Please manually provide the setup link to the teacher.";
                    TempData["SetupLink"] = setupUrl;
                }

                return RedirectToAction(nameof(Teachers));
            }

            return View(teacher);
        }

        private string GeneratePasswordSetupToken()
        {
            var tokenBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tokenBytes);
            }
            return Convert.ToBase64String(tokenBytes);
        }

        // Student Management
        public async Task<IActionResult> Students()
        {
            var students = await _context.Users
                .Where(u => u.Role == UserRole.Student)
                .ToListAsync();
            return View(students);
        }

        public IActionResult AddStudent()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddStudent(User student)
        {
            try
            {
                Console.WriteLine($"AddStudent action started for email: {student?.Email}");
                Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

                if (!ModelState.IsValid)
                {
                    foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        Console.WriteLine($"Model Error: {modelError.ErrorMessage}");
                    }
                return View(student);
        }

                // Check if email already exists
                if (student == null || await _context.Users.AnyAsync(u => u.Email == student.Email))
                {
                    Console.WriteLine($"Email already exists: {student?.Email}");
                    ModelState.AddModelError("Email", "Email already exists");
                    return View(student);
                }

                Console.WriteLine("Generating password setup token...");
                // Generate a unique token for password setup
                var token = GeneratePasswordSetupToken();
                
                Console.WriteLine("Setting up student account...");
                // Set the password reset token and expiry
                student.PasswordResetToken = token;
                student.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24);
                student.CreatedAt = DateTime.UtcNow;
                student.Role = UserRole.Student;
                student.IsActive = false; // Student will be activated after setting password

                Console.WriteLine("Adding student to database...");
                _context.Users.Add(student);
                await _context.SaveChangesAsync();
                Console.WriteLine("Student added to database successfully.");

                // Generate password setup URL
                var baseUrl = _configuration["ApplicationUrl"];
                Console.WriteLine($"Base URL from configuration: {baseUrl}");
                if (string.IsNullOrEmpty(baseUrl))
                {
                    baseUrl = $"{Request.Scheme}://{Request.Host}";
                    Console.WriteLine($"Using request-based URL: {baseUrl}");
                }
                var setupUrl = $"{baseUrl}/Account/SetStudentPassword?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(student.Email)}";
                Console.WriteLine($"Setup URL generated: {setupUrl}");

                try
                {
                    Console.WriteLine("Attempting to send password setup email...");
                    // Send password setup email
                    await _emailService.SendPasswordSetupEmailAsync(
                        student.Email,
                        student.FirstName,
                        setupUrl
                    );

                    Console.WriteLine("Email sent successfully!");
                    TempData["SuccessMessage"] = "Student added successfully. A password setup email has been sent.";
                }
                catch (Exception ex)
                {
                    // Log the error
                    Console.WriteLine($"Error sending email: {ex.Message}");
                    Console.WriteLine($"Error Type: {ex.GetType().FullName}");
                    Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    
                    // Still show success but with a warning about email
                    TempData["SuccessMessage"] = "Student added successfully.";
                    TempData["WarningMessage"] = "Could not send the password setup email. Please manually provide the setup link to the student.";
                    TempData["SetupLink"] = setupUrl;
                }

                return RedirectToAction(nameof(Students));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error in AddStudent: {ex.Message}");
                Console.WriteLine($"Error Type: {ex.GetType().FullName}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                ModelState.AddModelError("", "An unexpected error occurred while adding the student.");

                return View(student);
        }
    }

        public IActionResult AddCourse()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCourse(Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Courses.Add(course);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Course added successfully.";
                return RedirectToAction(nameof(ManageCourse));
            }
            return View(course);
        }

        public async Task<IActionResult> ManageCourse()
        {
            var courses = await _context.Courses.ToListAsync();
            return View(courses);
        }

        // GET: Admin/CourseWiseSubject
        public async Task<IActionResult> CourseWiseSubject()
        {
            var courses = await _context.Courses
                .Include(c => c.CourseSubjects)
                .ThenInclude(cs => cs.Subject)
                .ToListAsync();
            return View(courses);
        }

        public IActionResult Course()
        {
            return RedirectToAction(nameof(ManageCourse));
        }

        // Subject Management
        public async Task<IActionResult> Subjects()
        {
            var subjects = await _context.Subjects.ToListAsync();
            return View(subjects);
        }

        // GET: Admin/AssignSubjectsToCourse/5
        public async Task<IActionResult> AssignSubjectsToCourse(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.CourseSubjects)
                .ThenInclude(cs => cs.Subject)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return NotFound();
            }

            var allSubjects = await _context.Subjects.ToListAsync();

            ViewBag.AllSubjects = allSubjects;
            return View(course);
        }

        // POST: Admin/AssignSubjectsToCourse/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignSubjectsToCourse(int courseId, int[] subjectIds)
        {
            var course = await _context.Courses
                .Include(c => c.CourseSubjects)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
            {
                return NotFound();
            }

            // Remove existing assignments
            _context.CourseSubjects.RemoveRange(course.CourseSubjects);

            // Add new assignments
            if (subjectIds?.Length > 0)
            {
                foreach (var subjectId in subjectIds)
                {
                    var assignment = new CourseSubject
                    {
                        CourseId = courseId,
                        SubjectId = subjectId
                    };
                    _context.CourseSubjects.Add(assignment);
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Subject assignments updated successfully.";
            return RedirectToAction(nameof(CourseWiseSubject));
        }

        public IActionResult AddSubject()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddSubject(Subject subject)
        {
            if (ModelState.IsValid)
            {
                subject.IsActive = true;
                _context.Subjects.Add(subject);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Subjects));
            }

            return View(subject);
        }

        // Teacher Subject Allocation
        public async Task<IActionResult> AllocateSubjects()
        {
            var teachers = await _context.Teachers
                .Include(t => t.TeacherSubjects)
                .ThenInclude(ts => ts.Subject)
                .OrderBy(t => t.FirstName)
                .ToListAsync();

            return View(teachers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignSubjectsToTeacher(int teacherId, int[] subjectIds)
        {
            var teacher = await _context.Teachers
                .Include(t => t.TeacherSubjects)
                .FirstOrDefaultAsync(t => t.Id == teacherId);

            if (teacher == null)
            {
                return NotFound();
            }

            // Initialize TeacherSubjects collection if null
            if (teacher.TeacherSubjects == null)
            {
                teacher.TeacherSubjects = new List<TeacherSubject>();
            }

            // Remove existing assignments
            var existingAssignments = teacher.TeacherSubjects.ToList();
            _context.TeacherSubjects.RemoveRange(existingAssignments);

            // Add new assignments
            if (subjectIds?.Length > 0)
            {
                foreach (var subjectId in subjectIds)
                {
                    var assignment = new TeacherSubject
                    {
                        TeacherId = teacherId,
                        SubjectId = subjectId,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    teacher.TeacherSubjects.Add(assignment);
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Subject assignments updated successfully.";
            return RedirectToAction(nameof(Teachers));
        }

        private string GeneratePasswordResetToken()
        {
            var tokenBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(tokenBytes);
            return Convert.ToBase64String(tokenBytes);
        }

        // GET: Admin/EditTeacher/5
        public async Task<IActionResult> EditTeacher(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }

        // POST: Admin/EditTeacher/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTeacher(int id, Teacher teacher)
        {
            if (id != teacher.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTeacher = await _context.Teachers.FindAsync(id);
                    if (existingTeacher == null)
                    {
                        return NotFound();
                    }

                    // Check if email is being changed and if it already exists
                    if (existingTeacher.Email != teacher.Email &&
                        await _context.Teachers.AnyAsync(t => t.Email == teacher.Email))
                    {
                        ModelState.AddModelError("Email", "Email already exists");
                        return View(teacher);
                    }

                    existingTeacher.FirstName = teacher.FirstName;
                    existingTeacher.LastName = teacher.LastName;
                    existingTeacher.Email = teacher.Email;
                    existingTeacher.IsActive = teacher.IsActive;

                    // Only update password if a new one is provided
                    if (!string.IsNullOrEmpty(teacher.Password))
                    {
                        existingTeacher.Password = BCrypt.Net.BCrypt.HashPassword(teacher.Password);
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Teacher updated successfully";
                    return RedirectToAction(nameof(Teachers));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Teachers.AnyAsync(e => e.Id == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(teacher);
        }

        // GET: Admin/DeleteTeacher/5
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.TeacherSubjects)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                return NotFound();
            }

            // Remove teacher subjects first
            _context.TeacherSubjects.RemoveRange(teacher.TeacherSubjects);
            
            // Then remove the teacher
            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Teacher deleted successfully.";
            return RedirectToAction(nameof(Teachers));
        }

        // GET: Admin/AssignSubjects/5
        public async Task<IActionResult> AssignSubjects(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.TeacherSubjects)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                return NotFound();
            }

            ViewBag.Subjects = await _context.Subjects.ToListAsync();
            return View(teacher);
        }

        // POST: Admin/AssignSubjects/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignSubjects(int id, int[] subjectIds)
        {
            var teacher = await _context.Teachers
                .Include(t => t.TeacherSubjects)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                return NotFound();
            }

            // Remove existing assignments
            _context.TeacherSubjects.RemoveRange(teacher.TeacherSubjects);

            // Add new assignments
            if (subjectIds != null)
            {
                foreach (var subjectId in subjectIds)
                {
                    teacher.TeacherSubjects.Add(new TeacherSubject
                    {
                        TeacherId = id,
                        SubjectId = subjectId,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Subjects assigned successfully";
            return RedirectToAction(nameof(Teachers));
        }

        [HttpPost]
        public async Task<IActionResult> UnassignSubject(int teacherId, int subjectId)
        {
            var teacherSubject = await _context.TeacherSubjects
                .Include(ts => ts.Teacher)
                .Include(ts => ts.Subject)
                .FirstOrDefaultAsync(ts => ts.TeacherId == teacherId && ts.SubjectId == subjectId);

            if (teacherSubject == null)
            {
                return NotFound();
            }

            _context.TeacherSubjects.Remove(teacherSubject);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(AllocateSubjects));
        }

        // GET: Admin/EditStudent/5
        public async Task<IActionResult> EditStudent(int id)
        {
            var student = await _context.Users.FindAsync(id);
            if (student == null || student.Role != UserRole.Student)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Admin/EditStudent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(int id, User student)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingStudent = await _context.Users.FindAsync(id);
                    if (existingStudent == null || existingStudent.Role != UserRole.Student)
                    {
                        return NotFound();
                    }

                    // Check if email is being changed and if it already exists
                    if (existingStudent.Email != student.Email &&
                        await _context.Users.AnyAsync(u => u.Email == student.Email && u.Id != id))
                    {
                        ModelState.AddModelError("Email", "Email already exists");
                        return View(student);
                    }

                    existingStudent.FirstName = student.FirstName;
                    existingStudent.LastName = student.LastName;
                    existingStudent.Email = student.Email;
                    existingStudent.IsActive = student.IsActive;

                    // Only update password if a new one is provided
                    if (!string.IsNullOrEmpty(student.Password))
                    {
                        existingStudent.Password = BCrypt.Net.BCrypt.HashPassword(student.Password);
                    }

                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Student updated successfully";
                    return RedirectToAction(nameof(Students));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _context.Users.AnyAsync(e => e.Id == id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return View(student);
        }

        // GET: Admin/DeleteStudent/5
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRole.Student);

            if (student == null)
            {
                return NotFound();
            }

            // Remove the student
            _context.Users.Remove(student);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Student deleted successfully.";
            return RedirectToAction(nameof(Students));
        }

        // GET: Admin/ViewTeacherSubjects/5
        public async Task<IActionResult> ViewTeacherSubjects(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.TeacherSubjects)
                .ThenInclude(ts => ts.Subject)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher == null)
            {
                return NotFound();
            }

            return View(teacher);
        }
    }
}
