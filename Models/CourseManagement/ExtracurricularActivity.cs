using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolSystem.Models.CourseManagement
{
    public class ExtracurricularActivity
    {
        [Key]
        public int EA_Id { get; set; }

        [Required]
        [ForeignKey("Activity")]
        public int ActivityId { get; set; }

        [Required]
        [ForeignKey("Course")]
        public int CourseId { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdateAt { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public virtual Activity? Activity { get; set; }
        public virtual Course? Course { get; set; }
    }
}
