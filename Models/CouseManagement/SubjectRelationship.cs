using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.CouseManagement
{
    public class SubjectRelationship
    {
        [Key]
        public int RelationshipId { get; set; } // Primary Key

        [Required]
        public int AfterSubjectId { get; set; } // รหัสวิชาที่เกิดขึ้นหลัง

        [Required]
        public int BeforeSubjectId { get; set; } // รหัสวิชาที่เกิดขึ้นก่อน

        public Subjects? AfterSubject { get; set; }
        public Subjects? BeforeSubject { get; set; }
    }
}
