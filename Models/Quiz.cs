using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduQuiz_.Models
{
    public class Quiz
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Password { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public int Attempts { get; set; }

        [ForeignKey("Unit")]
        public int UnitId { get; set; }

        public Unit? Unit { get; set; }

        public int DurationMinutes { get; set; } = 15; // default duration in minutes
    }
}
