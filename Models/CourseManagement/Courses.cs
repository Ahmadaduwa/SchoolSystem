using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.CourseManagement
{
    public class Courses
    {
        [Key]
        public int CourseId { get; set; } // Primary Key

        [Required]
        [MaxLength(200)] // จำกัดความยาวสูงสุดของชื่อวิชา
        public string? CourseName { get; set; }

        [MaxLength(500)] // จำกัดความยาวของคำอธิบาย
        public string? Description { get; set; }

        [Required]
        public DateTime CreateAt { get; set; } // วันที่สร้าง

        public DateTime? UpdateAt { get; set; } // วันที่อัปเดต (เป็นค่าว่างได้)
    }
}
