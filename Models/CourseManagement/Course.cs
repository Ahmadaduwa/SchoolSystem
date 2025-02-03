using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SchoolSystem.Models.CurriculumManagement;

namespace SchoolSystem.Models.CourseManagement
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; } // Primary Key

        [Required]
        [MaxLength(50)] // กำหนดความยาวสูงสุดของ Subject Code
        public string? Course_Code { get; set; }

        [Required]
        [MaxLength(200)] // กำหนดความยาวสูงสุดของชื่อรายวิชา
        public string? CourseName { get; set; }
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? Objective { get; set; } // วัตถุประสงค์ของรายวิชา

        public int Unit { get; set; } // จำนวนหน่วยกิต

        [Required]
        [StringLength(20)]
        public required string Status { get; set; } // Status (e.g., Active/Inactive)

        // Foreign Key เชื่อมกับตาราง SubjectCategory
        [ForeignKey("CourseCategory")]
        public int CourseCategoryId { get; set; }
        public required CourseCategory CourseCategory { get; set; } // Navigation Property

        // Relationships
        public virtual ICollection<ClassManagement.ClassManagement> ClassManagements { get; set; } = new List<ClassManagement.ClassManagement>();
        public virtual ICollection<ElectiveCourse> ElectiveCourses { get; set; } = new List<ElectiveCourse>();
        public virtual ICollection<CompulsoryCourse> CompulsoryCourses { get; set; } = new List<CompulsoryCourse>();
        public virtual ICollection<CompulsoryElectiveCourse> CompulsoryElectiveCourses { get; set; } = new List<CompulsoryElectiveCourse>();
    }
}
