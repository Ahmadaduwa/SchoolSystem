using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace SchoolSystem.Models.ClassManagement
{
    public class Class
    {
        [Key]
        public int ClassID { get; set; } // Primary Key

        [Required]
        public int GradeLevelId { get; set; } // Foreign Key to GradeLevels

        [Required]
        [StringLength(100)]
        public required string ClassName { get; set; } // Name of the class

        [Required]
        [Range(1, int.MaxValue)]
        public int Capacity { get; set; } // Maximum number of students
        public virtual ICollection<ClassManagement> ClassManagements { get; set; } = new List<ClassManagement>();
    }
}
