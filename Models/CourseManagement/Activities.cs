using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.CourseManagement
{
    public class Activities
    {
        [Key]
        public int ActivityId { get; set; }

        [Required]
        [StringLength(100)]
        public string? ActivityName { get; set; }
        [Required]
        [StringLength(250)] 
        public string? Description { get; set; }

        public ICollection<ExtracurricularActivity>? ExtracurricularActivities { get; set; } 
    }
}
