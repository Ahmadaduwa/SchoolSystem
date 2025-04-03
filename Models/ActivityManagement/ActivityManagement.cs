using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolSystem.Models.ClassManagement;

namespace SchoolSystem.Models.ActivityManagement
{
    public class ActivityManagement
    {
        [Key]
        public int AM_id { get; set; }

        [Required]
        public int ActivityId { get; set; }
        [ForeignKey("ActivityId")]
        public virtual Activity? Activity { get; set; }

        [Required]
        public int CheckCount { get; set; }

        [Required]
        public int SemesterId { get; set; }
        [ForeignKey("SemesterId")]
        public virtual Semester? Semester { get; set; }

        [Required]
        [StringLength(50)]
        public string? Type { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<ActivityAttendance> ActivityAttendance { get; set; } = new List<ActivityAttendance>();
        public virtual ICollection<ActivitySchedule> ActivitySchedule { get; set; } = new List<ActivitySchedule>(); 
        public virtual ICollection<ActivityAttendanceSummary> AcitivityAttendanceSummary { get; set; } = new List<ActivityAttendanceSummary>();
        public virtual ICollection<ActivityAttendanceCheck> ActivityAttendanceCheck { get; set; } = new List<ActivityAttendanceCheck>();
    }
}
