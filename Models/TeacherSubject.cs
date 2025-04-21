using System.ComponentModel.DataAnnotations;

namespace EduQuiz_.Models
{
    public class TeacherSubject
    {
        public int Id { get; set; }

        [Required]
        public int TeacherId { get; set; }
        public Teacher? Teacher { get; set; }

        [Required]
        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 