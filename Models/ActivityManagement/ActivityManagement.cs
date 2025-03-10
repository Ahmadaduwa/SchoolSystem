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
        public DateTime? UpdateAt { get; set; }
        public virtual ICollection<ActivityAttendance> ActivityAttendance { get; set; } = new List<ActivityAttendance>();
        public virtual ICollection<ActivitySchedule> ActivitySchedule { get; set; } = new List<ActivitySchedule>();
    }
}
