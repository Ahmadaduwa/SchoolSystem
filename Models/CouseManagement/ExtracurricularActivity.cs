    using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.CouseManagement
{
    public class ExtracurricularActivity
    {
        [Key]
        public int EA_Id { get; set; } // Primary Key

        [Required]
        public int ActivityId { get; set; } // Foreign Key ไปยังตาราง Activities

        [Required]
        public int CourseId { get; set; } // Foreign Key ไปยังตาราง Courses

        public ICollection<Activities>? Activity { get; set; }
        public ICollection<Courses>? Course { get; set; }
    }
}
