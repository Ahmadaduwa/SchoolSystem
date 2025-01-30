using System.ComponentModel.DataAnnotations;
using SchoolSystem.Models.ClassManagement;
using SchoolSystem.Models.CourseManagement;
using SchoolSystem.Models.CurriculumManagement;

namespace SchoolSystem.Models.CurriculumManagement
{
    public class CompulsoryCourse
    {
        [Key]
        public int CC_Id { get; set; } // Primary Key

        [Required]
        public int GradeLevelId { get; set; } // Foreign Key ไปยัง GradeLevels

        [Required]
        public int CurriculumId { get; set; } // Foreign Key ไปยัง Courses

        [Required]
        public int CourseId { get; set; } // Foreign Key ไปยัง Subjects

        public ICollection<GradeLevels>? GradeLevel { get; set; }
        public ICollection<Course>? Curriculum { get; set; }
        public ICollection<Curriculum>? Course { get; set; }
    }
}
