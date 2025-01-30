using System.ComponentModel.DataAnnotations;
using SchoolSystem.Models.CurriculumManagement;

namespace SchoolSystem.Models.CourseManagement
{
    public class CourseCategory
    {
        [Key]
        public int CourseCategoryId { get; set; } // Primary Key

        [Required]
        [MaxLength(100)] // กำหนดความยาวสูงสุดของชื่อหมวดหมู่
        public string? Name { get; set; }

        [MaxLength(500)] // คำอธิบายหมวดหมู่
        public string? Description { get; set; }

        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
