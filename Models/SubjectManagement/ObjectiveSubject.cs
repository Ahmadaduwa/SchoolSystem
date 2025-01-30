using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.SubjectManagement
{
    public class ObjectiveSubject
    {
        [Key]
        public int OS_ID { get; set; } // Primary Key

        [Required]
        public int SubjectId { get; set; } // Foreign Key ไปยังตาราง Subject

        [Required]
        public int ObjectiveID { get; set; } // Foreign Key ไปยังตาราง Objectives

        public ICollection<Subjects>? Subject { get; set; }
        public ICollection<Objectives>? Objective { get; set; }
    }
}
