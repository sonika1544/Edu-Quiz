using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EduQuiz_.Data;
using EduQuiz_.Models;

namespace EduQuiz_.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var subjects = await _context.Subjects
                .Where(s => s.IsActive)
                .ToListAsync();

            return View(subjects);
        }
    }
} 