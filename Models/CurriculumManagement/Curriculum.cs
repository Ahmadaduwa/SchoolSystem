using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.CurriculumManagement
{
    public class Curriculum
    {
        [Key]
        public int CurriculumId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Curriculum_Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string CurriculumName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdateAt { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        public virtual ICollection<ExtracurricularActivity> ExtracurricularActivities { get; set; } = new List<ExtracurricularActivity>();
        public virtual ICollection<ElectiveCourse> ElectiveCourses { get; set; } = new List<ElectiveCourse>();
        public virtual ICollection<CompulsoryCourse> CompulsoryCourses { get; set; } = new List<CompulsoryCourse>();
        public virtual ICollection<CompulsoryElectiveCourse> CompulsoryElectiveCourses { get; set; } = new List<CompulsoryElectiveCourse>();

    }
}
