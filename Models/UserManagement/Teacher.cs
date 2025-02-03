using System;
using System.ComponentModel.DataAnnotations;
using SchoolSystem.Models.ClassManagement;

namespace SchoolSystem.Models.UserManagement
{
    public class Teacher
    {
        [Key]
        public int TeacherId { get; set; }

        [Required]
        public string? UserId { get; set; }

        public int DepartmentId { get; set; }

        [Required]
        public DateTime HireDate { get; set; } // แทน DateOnly เพื่อรองรับ EF Core

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive value.")]
        public decimal Salary { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active"; // ค่าเริ่มต้น

        public DateTime UpdateAt { get; set; } = DateTime.UtcNow;
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public virtual ICollection<ClassManagement.ClassManagement> ClassManagements { get; set; } = new List<ClassManagement.ClassManagement>();
    }
}
