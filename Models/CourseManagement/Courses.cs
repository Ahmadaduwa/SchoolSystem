using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolSystem.Models.CourseManagement
{
    public class Courses
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int CourseId { get; set; } // Primary Key

        [Required]
        [MaxLength(50)] // กำหนดความยาวสูงสุดของ Subject Code
        public string? Course_Code { get; set; }

        [Required]
        [MaxLength(200)] // จำกัดความยาวสูงสุดของชื่อวิชา
        public string? CourseName { get; set; }

        [MaxLength(500)] // จำกัดความยาวของคำอธิบาย
        public string? Description { get; set; }

        [Required]
        public DateTime CreateAt { get; set; } // วันที่สร้าง

        public DateTime? UpdateAt { get; set; } // วันที่อัปเดต (เป็นค่าว่างได้)

        [Required]
        [MaxLength(50)] // จำกัดความยาวของสถานะ
        public string Status { get; set; } = "Active"; // ค่าเริ่มต้นเป็น "Active"
    }
}
