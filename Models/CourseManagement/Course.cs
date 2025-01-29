using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolSystem.Models.CourseManagement
{
    public class Course
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Required]
        public int CourseId { get; set; } 

        [Required]
        [MaxLength(50)] 
        public string? Course_Code { get; set; }

        [Required]
        [MaxLength(200)] 
        public string? CourseName { get; set; }

        [MaxLength(500)] 
        public string? Description { get; set; }

        [Required]
        public DateTime CreateAt { get; set; } 

        public DateTime? UpdateAt { get; set; } 

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Active";

        public ICollection<ExtracurricularActivity>? ExtracurricularActivities { get; set; }
    }
}
