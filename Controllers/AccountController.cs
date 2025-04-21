using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using EduQuiz_.Data;
using EduQuiz_.Models;
using EduQuiz_.Services;
using System.Security.Cryptography;
using System.Text;

namespace EduQuiz_.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public AccountController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Email and password are required");
                return View();
            }

            // First check if it's an admin user
            var adminUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Role == UserRole.Admin);

            if (adminUser != null && BCrypt.Net.BCrypt.Verify(password, adminUser.Password))
            {
                if (!adminUser.IsActive)
                {
                    ModelState.AddModelError("", "Your account is not active.");
                    return View();
                }

                var adminClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, adminUser.Email),
                    new Claim(ClaimTypes.Role, "Admin"),
                    new Claim("UserId", adminUser.Id.ToString())
                };

                var adminIdentity = new ClaimsIdentity(adminClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties();

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(adminIdentity),
                    authProperties);

                return RedirectToAction("Index", "Admin");
            }

            // Check if it's a student
            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Role == UserRole.Student);

            if (student != null && BCrypt.Net.BCrypt.Verify(password, student.Password))
            {
                if (!student.IsActive)
                {
                    ModelState.AddModelError("", "Your account is not active. Please check your email for activation instructions.");
                    return View();
                }

                var studentClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, student.Email),
                    new Claim(ClaimTypes.Role, "Student"),
                    new Claim("UserId", student.Id.ToString())
                };

                var studentIdentity = new ClaimsIdentity(studentClaims, CookieAuthenticationDefaults.AuthenticationScheme);
                var studentAuthProperties = new AuthenticationProperties();

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(studentIdentity),
                    studentAuthProperties);

                return RedirectToAction("Index", "Student");
            }

            // If not admin or student, check if it's a teacher
            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Email == email);

            if (teacher == null || !BCrypt.Net.BCrypt.Verify(password, teacher.Password))
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View();
            }

            if (!teacher.IsActive)
            {
                ModelState.AddModelError("", "Your account is not active. Please check your email for activation instructions.");
                return View();
            }

            var teacherClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, teacher.Email),
                new Claim(ClaimTypes.Role, "Teacher"),
                new Claim("UserId", teacher.Id.ToString())
            };

            var teacherIdentity = new ClaimsIdentity(teacherClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            var teacherAuthProperties = new AuthenticationProperties();

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(teacherIdentity),
                teacherAuthProperties);

            return RedirectToAction("Index", "Teacher");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task<IActionResult> SetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return BadRequest("Invalid request");
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Email == email);

            if (teacher == null)
            {
                return NotFound("Teacher not found");
            }

            if (teacher.PasswordResetToken != token || 
                !teacher.PasswordResetTokenExpiry.HasValue || 
                teacher.PasswordResetTokenExpiry.Value < DateTime.UtcNow)
            {
                return BadRequest("Invalid or expired token");
            }

            ViewBag.Email = email;
            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPassword(string token, string email, string password)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return BadRequest("Invalid request");
            }

            var teacher = await _context.Teachers
                .FirstOrDefaultAsync(t => t.Email == email);

            if (teacher == null)
            {
                return NotFound("Teacher not found");
            }

            if (teacher.PasswordResetToken != token || 
                !teacher.PasswordResetTokenExpiry.HasValue || 
                teacher.PasswordResetTokenExpiry.Value < DateTime.UtcNow)
            {
                return BadRequest("Invalid or expired token");
            }

            teacher.Password = BCrypt.Net.BCrypt.HashPassword(password);
            teacher.PasswordResetToken = null;
            teacher.PasswordResetTokenExpiry = null;
            teacher.IsActive = true;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password set successfully. You can now log in.";
            return RedirectToAction("Login", "Account");
        }

        public async Task<IActionResult> SetStudentPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return BadRequest("Invalid request");
            }

            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Role == UserRole.Student);

            if (student == null)
            {
                return NotFound("Student not found");
            }

            if (student.PasswordResetToken != token || 
                !student.PasswordResetTokenExpiry.HasValue || 
                student.PasswordResetTokenExpiry.Value < DateTime.UtcNow)
            {
                return BadRequest("Invalid or expired token");
            }

            ViewBag.Email = email;
            ViewBag.Token = token;
            return View("SetPassword"); // Reuse the same view as teachers
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStudentPassword(string token, string email, string password)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return BadRequest("Invalid request");
            }

            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Role == UserRole.Student);

            if (student == null)
            {
                return NotFound("Student not found");
            }

            if (student.PasswordResetToken != token || 
                !student.PasswordResetTokenExpiry.HasValue || 
                student.PasswordResetTokenExpiry.Value < DateTime.UtcNow)
            {
                return BadRequest("Invalid or expired token");
            }

            student.Password = BCrypt.Net.BCrypt.HashPassword(password);
            student.PasswordResetToken = null;
            student.PasswordResetTokenExpiry = null;
            student.IsActive = true;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Password set successfully. You can now log in.";
            return RedirectToAction("Login", "Account");
        }

        private string GeneratePasswordResetToken()
        {
            var tokenBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(tokenBytes);
            return Convert.ToBase64String(tokenBytes);
        }
    }
} 