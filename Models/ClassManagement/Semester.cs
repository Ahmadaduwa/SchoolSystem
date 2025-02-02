using System;
using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.ClassManagement
{
    public class Semester
    {
        [Key]
        public int SemesterID { get; set; } // Primary Key

        [Required]
        [StringLength(10)]
        public required string SemesterYear { get; set; } // Year of the semester (e.g., 2025)

        [Required]
        [Range(1, 3)]
        public int SemesterNumber { get; set; } // Semester number (e.g., 1 for Spring, 2 for Summer, 3 for Fall)

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; } // Start date of the semester

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime EndTime { get; set; } // End date of the semester

        // Navigation Properties
        public virtual ICollection<ClassManagement> ClassManagements { get; set; } = new List<ClassManagement>();

    }
}
