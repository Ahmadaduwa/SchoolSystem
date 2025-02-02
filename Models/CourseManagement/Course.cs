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

        [MaxLength(500)] // คำอธิบายรายวิชา
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? Objective { get; set; } // วัตถุประสงค์ของรายวิชา

        [Required]
        public int Unit { get; set; } // จำนวนหน่วยกิต

        // Foreign Key เชื่อมกับตาราง SubjectCategory
        [ForeignKey("SubjectCategory")]
        public int CourseCategoryId { get; set; }
        public required CourseCategory CourseCategory { get; set; } // Navigation Property
        public virtual ICollection<ElectiveCourse> ElectiveCourses { get; set; } = new List<ElectiveCourse>();
        public virtual ICollection<CompulsoryCourse> CompulsoryCourses { get; set; } = new List<CompulsoryCourse>();
        public virtual ICollection<CompulsoryElectiveCourse> CompulsoryElectiveCourses { get; set; } = new List<CompulsoryElectiveCourse>();
    }
}
