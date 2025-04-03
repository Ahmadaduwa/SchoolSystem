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

        [Required]
        [MaxLength(50)]
        public string? Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
    }
}
