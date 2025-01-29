using System.ComponentModel.DataAnnotations;

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

    }
}
