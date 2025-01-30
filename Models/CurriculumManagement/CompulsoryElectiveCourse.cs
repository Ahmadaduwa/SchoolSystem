using System.ComponentModel.DataAnnotations;
using SchoolSystem.Models.ClassManagement;
using SchoolSystem.Models.CourseManagement;

namespace SchoolSystem.Models.CurriculumManagement
{
    public class CompulsoryElectiveCourse
    {
        [Key]
        public int CEC_Id { get; set; } // Primary Key

        public int GradeLevelId { get; set; } // Foreign Key ไปยัง GradeLevels

        [Required]
        public int CurriculumId { get; set; } // Foreign Key ไปยัง Courses

        [Required]
        public int CourseId { get; set; } // Foreign Key ไปยัง Subjects

        public virtual GradeLevels? GradeLevel { get; set; }
        public virtual Course? Course { get; set; }
        public virtual Curriculum? Curriculum { get; set; }
    }
}
