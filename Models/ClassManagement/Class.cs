using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolSystem.Models.ClassManagement
{
    public class Class
    {
        [Key]
        public int ClassId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Class number must be a positive value.")]
        public int ClassNumber { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Capacity { get; set; }

        public DateTime CreateAt { get; set; }

        [Required]
        [ForeignKey("GradeLevels")]
        public int GradeLevelId { get; set; }

        public virtual GradeLevels? GradeLevels { get; set; }
        public virtual ICollection<ClassManagement> ClassManagement { get; set; } = new List<ClassManagement>();

    }
}
