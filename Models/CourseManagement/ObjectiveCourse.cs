using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.CourseManagement
{
    public class ObjectiveCourse
    {
        [Key]
        public int OC_ID { get; set; } // Primary Key

        [Required]
        public int CourseId { get; set; } // Foreign Key ไปยังตาราง Subject

        [Required]
        public int ObjectiveID { get; set; } // Foreign Key ไปยังตาราง Objectives

        public ICollection<Course>? Course { get; set; }
        public ICollection<Objectives>? Objective { get; set; }
    }
}
