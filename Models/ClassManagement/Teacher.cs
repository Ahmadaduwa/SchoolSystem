using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.ClassManagement
{
    public class Teacher
    {
        [Key]
        public int TeacherID { get; set; } // Primary Key

        [Required]
        public int UserId { get; set; } // Links to a User entity (if you have one)

        [Required]
        [StringLength(255)]
        public required string TeacherName { get; set; }

        [Required]
        [StringLength(50)]
        public required string Status { get; set; } // Employment status (e.g., Active, Retired)

        [Required]
        public int DepartmentId { get; set; } // Department ID (could reference another table)

        [Required]
        public DateTime UpdatedAt { get; set; } // Timestamp for the last update

        // Navigation Properties
        public virtual ICollection<ClassManagement> ClassManagements { get; set; } = new List<ClassManagement>();

    }
}
