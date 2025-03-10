using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolSystem.Models.UserManagement
{
    public class Teacher
    {
        [Key]
        public int TeacherId { get; set; }

        [Required]
        public int? ProfileId { get; set; } // ✅ เปลี่ยนจาก int เป็น int?

        [ForeignKey("ProfileId")]
        public Profiles? Profile { get; set; }

        public int? DepartmentId { get; set; }

        [Required]
        public DateTime HireDate { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Salary must be a positive value.")]
        public decimal Salary { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active";

        public DateTime UpdateAt { get; set; } = DateTime.UtcNow;
        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        // ✅ Navigation Properties
        public virtual ICollection<ClassManagement.ClassManagement> ClassManagements { get; set; } = new List<ClassManagement.ClassManagement>();
    }
}
