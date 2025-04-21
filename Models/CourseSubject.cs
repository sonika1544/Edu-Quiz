using System.ComponentModel.DataAnnotations;

namespace EduQuiz_.Models
{
    public class CourseSubject
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }
        public Course Course { get; set; } = null!;

        [Required]
        public int SubjectId { get; set; }
        public Subject Subject { get; set; } = null!;
    }
}
