using System.ComponentModel.DataAnnotations;

namespace EduQuiz_.Models
{
    public class Course
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string CourseName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public ICollection<CourseSubject> CourseSubjects { get; set; } = new List<CourseSubject>();
    }
}
