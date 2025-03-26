using System.ComponentModel.DataAnnotations;
using SchoolSystem.Models.CurriculumManagement;

namespace SchoolSystem.Models.ActivityManagement
{
    public class Activity
    {
        [Key]
        public int ActivityId { get; set; }

        [Required(ErrorMessage = "กรุณากรอกชื่อกิจกรรม")]
        [StringLength(100, ErrorMessage = "ชื่อกิจกรรมต้องไม่เกิน 100 ตัวอักษร")]
        public string ActivityName { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณากรอกรายละเอียด")]
        [StringLength(250, ErrorMessage = "รายละเอียดต้องไม่เกิน 250 ตัวอักษร")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "กรุณาเลือกประเภทกิจกรรม")]
        [RegularExpression("Daily|Special", ErrorMessage = "ประเภทกิจกรรมไม่ถูกต้อง ค่าที่อนุญาตคือ 'Daily' หรือ 'Special'")]
        public string ActivityType { get; set; } = "Special";

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdateAt { get; set; }

        [Required(ErrorMessage = "กรุณากรอกสถานะ")]
        [StringLength(20, ErrorMessage = "สถานะต้องไม่เกิน 20 ตัวอักษร")]
        [RegularExpression("Active|Inactive", ErrorMessage = "สถานะกิจกรรมไม่ถูกต้อง ค่าที่อนุญาตคือ 'Active' หรือ 'Inactive'")]
        public string Status { get; set; } = "Active";

        public virtual ICollection<ExtracurricularActivity> ExtracurricularActivities { get; set; } = new List<ExtracurricularActivity>();
        public virtual ICollection<ActivityManagement> ActivityManagement { get; set; } = new List<ActivityManagement>();
    }
}
