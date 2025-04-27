using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.Assignment
{
    public class Assignment
    {
        [Key]
        public int AssignmentId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public float FullScore { get; set; }

        [Required]
        public float RealScore { get; set; }

        [Required]
        public DateTime AssignedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public int CM_Id { get; set; }

        [ForeignKey("CM_Id")]
        public virtual ClassManagement.ClassManagement? ClassManagement { get; set; } 

        public virtual ICollection<AssignmentScore> AssignmentScores { get; set; } = new List<AssignmentScore>();

     
    }

}
