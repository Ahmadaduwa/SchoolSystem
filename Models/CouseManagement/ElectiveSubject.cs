using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.CouseManagement
{
    public class ElectiveSubject
    {
        [Key]
        public int ES_Id { get; set; } // Primary Key

        [Required]
        public int GradeLevelId { get; set; } // Foreign Key ไปยัง GradeLevels

        [Required]
        public int CourseId { get; set; } // Foreign Key ไปยัง Courses

        [Required]
        public int SubjectId { get; set; } // Foreign Key ไปยัง Subjects

        public ICollection<GradeLevels>? GradeLevel { get; set; }
        public ICollection<Courses>? Course { get; set; }
        public ICollection<Subjects>? Subject { get; set; }
    }
}
