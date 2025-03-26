using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolSystem.Models.UserManagement;

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

        [Required]
        [ForeignKey("GradeLevels")]
        public int GradeLevelId { get; set; }

        [Required]
        [MaxLength(50)]
        public string? Status { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; }

        public virtual GradeLevels? GradeLevels { get; set; }
        public virtual ICollection<ClassManagement> ClassManagement { get; set; } = new List<ClassManagement>();
        public virtual ICollection<Student> Student { get; set; } = new List<Student>();

    }
}
