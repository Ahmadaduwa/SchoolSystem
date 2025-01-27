using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models
{
    public class Course
    {
        [Key]
        [Display(Name = "CourseID")]
        public int CourseID { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "รหัสวิชา")]
        public required string CourseCode { get; set; }

        [Required]
        [StringLength(255)]
        [Display(Name = "คำอธิบายรายวิชา")]
        public required string CourseName { get; set; }

        [StringLength(1000)]
        [Display(Name = "วัตถุประสงค์การเรียนรู้")]
        public string? LearningObjective { get; set; }

        [Range(1, 10)]
        [Display(Name = "หน่วยการเรียน")]
        public int Units { get; set; }

        [StringLength(100)]
        [Display(Name = "หมวดวิชา")]
        public string? Category { get; set; }

        [StringLength(255)]
        [Display(Name = "รายวิชาเรียนก่อน")]
        public string? Prerequisites { get; set; }

        [StringLength(255)]
        [Display(Name = "รายวิชาเรียนหลัง")]
        public string? Postrequisites { get; set; }
    }
}
