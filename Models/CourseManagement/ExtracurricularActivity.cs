using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.CourseManagement
{
    public class ExtracurricularActivity
    {
        [Key]
        public int EA_Id { get; set; }

        [Required]
        public int ActivityId { get; set; }

        [Required]
        public int CourseId { get; set; } 

        public ICollection<Activities>? Activity { get; set; }
        public ICollection<Courses>? Course { get; set; }
    }
}
