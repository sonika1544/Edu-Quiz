using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EduQuiz_.Models
{
    public class Unit
    {
        [Key]
        public int Id { get; set; }

        public int UnitNumber { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public int SubjectId { get; set; }

        [ForeignKey("SubjectId")]
        public Subject? Subject { get; set; }
    }
}
