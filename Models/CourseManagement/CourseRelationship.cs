using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.SubjectManagement
{
    public class CourseRelationship
    {
        [Key]
        public int RelationshipId { get; set; } // Primary Key

        [Required]
        public int AfterSubjectId { get; set; } // รหัสวิชาที่เกิดขึ้นหลัง

        [Required]
        public int BeforeSubjectId { get; set; } // รหัสวิชาที่เกิดขึ้นก่อน

        public ICollection<Course>? AfterSubject { get; set; }
        public ICollection<Course>? BeforeSubject { get; set; }
    }
}
