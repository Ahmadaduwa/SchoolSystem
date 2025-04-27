using SchoolSystem.Models.UserManagement;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.Assignment
{
    public class AssignmentScore
    {
        [Key]
        public int ScoreId { get; set; }

        [Required]
        public int AssignmentId { get; set; }

        [ForeignKey("AssignmentId")]
        public virtual Assignment? Assignment { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student? Student { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Score must be between 0 and 100.")]
        public float Score { get; set; }

        public DateTime SubmittedDate { get; set; } = DateTime.UtcNow;

        // Custom validation logic
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Assignment == null)
            {
                yield return new ValidationResult(
                    "Assignment must be provided to validate the score.",
                    new[] { nameof(Assignment) });
            }
            else if (Score > Assignment.FullScore)
            {
                yield return new ValidationResult(
                    $"Score cannot exceed the full score of {Assignment.FullScore}.",
                    new[] { nameof(Score) });
            }
        }
    }

}
