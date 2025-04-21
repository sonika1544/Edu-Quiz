using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduQuiz_.Models
{
    public class Question
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int QuizId { get; set; }

        [ForeignKey("QuizId")]
        public Quiz Quiz { get; set; }

        [Required]
        [StringLength(500)]
        public string QuestionText { get; set; }

        [Required]
        [StringLength(200)]
        public string Option1 { get; set; }

        [Required]
        [StringLength(200)]
        public string Option2 { get; set; }

        [Required]
        [StringLength(200)]
        public string Option3 { get; set; }

        [Required]
        [StringLength(200)]
        public string Option4 { get; set; }

        [Required]
        [StringLength(200)]
        public string CorrectAnswer { get; set; }
    }
}
