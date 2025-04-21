using System.ComponentModel.DataAnnotations;

namespace EduQuiz_.Models
{
    public class Subject
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public required string Name { get; set; }

        [Required]
        [StringLength(500)]
        public required string Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<CourseSubject> CourseSubjects { get; set; } = new List<CourseSubject>();
    }
} 