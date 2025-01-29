using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.CourseManagement
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Course_Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CourseName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdateAt { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public virtual ICollection<ExtracurricularActivity> ExtracurricularActivities { get; set; } = new List<ExtracurricularActivity>();
    }
}
