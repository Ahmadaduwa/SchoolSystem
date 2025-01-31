 using System.ComponentModel.DataAnnotations;
using SchoolSystem.Models.CurriculumManagement;

namespace SchoolSystem.Models.ClassManagement
{
    public class GradeLevels
    {
        [Key]
        public int GradeLevelId { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; }

        public virtual ICollection<ElectiveCourse> ElectiveCourses { get; set; } = new List<ElectiveCourse>();
        public virtual ICollection<CompulsoryCourse> CompulsoryCourses { get; set; } = new List<CompulsoryCourse>();
        public virtual ICollection<CompulsoryElectiveCourse> CompulsoryElectiveCourses { get; set; } = new List<CompulsoryElectiveCourse>();
        public virtual ICollection<Class> Classes { get; set; } = new List<Class>(); 

    }
}
