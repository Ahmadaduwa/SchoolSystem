using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolSystem.Models.ClassManagement;
using SchoolSystem.Models.SubjectManagement;

namespace SchoolSystem.Models.CourseManagement
{
    public class CompulsoryElectiveSubject
    {
        [Key]
        public int CES_Id { get; set; } // Primary Key

        public int GradeLevelId { get; set; } // Foreign Key ไปยัง GradeLevels

        [Required]
        public int CurriculumId { get; set; } // Foreign Key ไปยัง Curriculum

        [Required]
        public int CourseId { get; set; } // Foreign Key ไปยัง Courses

        [ForeignKey("GradeLevelId")]
        public GradeLevels GradeLevel { get; set; }

        [ForeignKey("CurriculumId")]
        public Curriculum Curriculum { get; set; }

        [ForeignKey("CourseId")]
        public Course Course { get; set; }
    }
}
