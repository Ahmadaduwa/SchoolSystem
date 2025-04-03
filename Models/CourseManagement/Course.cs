using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolSystem.Models.CurriculumManagement;
using SchoolSystem.Models.RegistrationManagement;

namespace SchoolSystem.Models.CourseManagement
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; } // Primary Key

        [Required]
        [MaxLength(50)]
        public string? Course_Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string? CourseName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [MaxLength(500)]
        public string? Objective { get; set; }

        [Required]
        [Range(0.5, 10.0, ErrorMessage = "Units must be between 0.5 and 10.0")]
        public float Unit { get; set; } // เปลี่ยนเป็น float

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active"; // ค่าเริ่มต้นเป็น Active

        [Required(ErrorMessage = "Please select a category.")]
        [ForeignKey("CourseCategory")]
        public int? CourseCategoryId { get; set; }
        public virtual CourseCategory? CourseCategory { get; set; }

        // 📌 Timestamp
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // เพิ่ม CreatedAt
        public DateTime? UpdatedAt { get; set; } // อัปเดตเวลาล่าสุดเมื่อมีการแก้ไข

        public virtual ICollection<ClassManagement.ClassManagement> ClassManagements { get; set; } = new List<ClassManagement.ClassManagement>();
        public virtual ICollection<ElectiveCourse> ElectiveCourses { get; set; } = new List<ElectiveCourse>();
        public virtual ICollection<CompulsoryCourse> CompulsoryCourses { get; set; } = new List<CompulsoryCourse>();
        public virtual ICollection<CompulsoryElectiveCourse> CompulsoryElectiveCourses { get; set; } = new List<CompulsoryElectiveCourse>();
        public virtual ICollection<RegisteredCourse> RegisteredCourse { get; set; } = new List<RegisteredCourse>();
    }
}
