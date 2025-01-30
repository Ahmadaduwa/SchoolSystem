using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolSystem.Models.ActivityManagement;

namespace SchoolSystem.Models.CurriculumManagement
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
        public int CurriculumId { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdateAt { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public virtual Activity? Activity { get; set; }
        public virtual Curriculum? Curriculum { get; set; }
    }
}
