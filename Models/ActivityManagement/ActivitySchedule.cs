using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolSystem.Models.ActivityManagement
{
    public class ActivitySchedule
    {
        [Key]
        public int AS_id { get; set; }

        [Required]
        public int AM_id { get; set; }

        [ForeignKey("AM_id")]
        public virtual ActivityManagement? ActivityManagement { get; set; }
        [Required]
        public DateTime? StartTime { get; set; }

        [Required]
        public DateTime? EndTime { get; set; }
    }
}
