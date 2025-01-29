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

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdateAt { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // กำหนดค่าเริ่มต้นเป็น Active

        public ICollection<ExtracurricularActivity>? ExtracurricularActivities { get; set; }
    }
}
