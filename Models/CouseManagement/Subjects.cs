using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.CouseManagement
{
    public class Subjects
    {
        [Key]
        public int SubjectId { get; set; } // Primary Key

        [Required]
        [MaxLength(50)] // กำหนดความยาวสูงสุดของ Subject Code
        public string? Subject_Code { get; set; }

        [Required]
        [MaxLength(200)] // กำหนดความยาวสูงสุดของชื่อรายวิชา
        public string? SubjectName { get; set; }

        [MaxLength(500)] // คำอธิบายรายวิชา
        public string? Description { get; set; }

        [Required]
        public int Unit { get; set; } // จำนวนหน่วยกิต

        // Foreign Key เชื่อมกับตาราง Courses
        [ForeignKey("Courses")]
        public int CourseId { get; set; }
        public required Courses Course { get; set; } // Navigation Property

        // Foreign Key เชื่อมกับตาราง SubjectCategory
        [ForeignKey("SubjectCategory")]
        public int SubjectCategoryId { get; set; }
        public required SubjectCategory SubjectCategory { get; set; } // Navigation Property
    }
}
