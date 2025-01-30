using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.SubjectManagement
{
    public class SubjectRelationship
    {
        [Key]
        public int RelationshipId { get; set; } // Primary Key

        [Required]
        public int AfterSubjectId { get; set; } // รหัสวิชาที่เกิดขึ้นหลัง

        [Required]
        public int BeforeSubjectId { get; set; } // รหัสวิชาที่เกิดขึ้นก่อน

        public ICollection<Subjects>? AfterSubject { get; set; }
        public ICollection<Subjects>? BeforeSubject { get; set; }
    }
}
