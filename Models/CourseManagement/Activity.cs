using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.CourseManagement
{
    public class Activity
    {
        [Key]
        public int ActivityId { get; set; }

        [Required]
        [StringLength(100)]
        public string ActivityName { get; set; } = string.Empty;

        [Required]
        [StringLength(250)]
        public string Description { get; set; } = string.Empty;

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdateAt { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public virtual ICollection<ExtracurricularActivity> ExtracurricularActivities { get; set; } = new List<ExtracurricularActivity>();
    }
}
