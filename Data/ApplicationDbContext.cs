using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using EduQuiz_.Models;

namespace EduQuiz_.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Unit> Units { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<TeacherSubject> TeacherSubjects { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseSubject> CourseSubjects { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Material> Materials { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<TeacherSubject>()
                .HasOne(ts => ts.Teacher)
                .WithMany(t => t.TeacherSubjects)
                .HasForeignKey(ts => ts.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeacherSubject>()
                .HasOne(ts => ts.Subject)
                .WithMany()
                .HasForeignKey(ts => ts.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CourseSubject>()
                .HasOne(cs => cs.Course)
                .WithMany(c => c.CourseSubjects)
                .HasForeignKey(cs => cs.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CourseSubject>()
                .HasOne(cs => cs.Subject)
                .WithMany(s => s.CourseSubjects)
                .HasForeignKey(cs => cs.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed admin user
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Email = "admin@eduquiz.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("Admin@123"), // Default password
                    FirstName = "Admin",
                    LastName = "User",
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}
