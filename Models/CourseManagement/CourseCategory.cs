using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.CourseManagement
{
    public class CourseCategory
    {
        [Key]
        public int SubjectCategoryId { get; set; } // Primary Key

        [Required]
        [MaxLength(100)] // กำหนดความยาวสูงสุดของชื่อหมวดหมู่
        public string? Name { get; set; }

        [MaxLength(500)] // คำอธิบายหมวดหมู่
        public string? Description { get; set; }
    }
}
