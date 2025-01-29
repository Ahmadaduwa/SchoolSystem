using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.CourseManagement
{
    public class ExtracurricularActivity
    {
        [Key]
        public int EA_Id { get; set; }

        [Required]
        public int ActivityId { get; set; }

        [Required]
        public int CourseId { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdateAt { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // กำหนดค่าเริ่มต้นเป็น Active

        public ICollection<Activities>? Activity { get; set; }
        public ICollection<Course>? Course { get; set; }
    }
}
