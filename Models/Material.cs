using System.ComponentModel.DataAnnotations;

namespace EduQuiz_.Models
{
    public class Material
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string FilePath { get; set; }

        public int SubjectId { get; set; }
        public Subject Subject { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;
    }
} 