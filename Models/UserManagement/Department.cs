using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.UserManagement
{
    public class Department
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Name { get; set; }

        [MaxLength(250)]
        public string? Description { get; set; } // เปลี่ยนจาก Des เป็น Description

        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(20)]
        public string? Status { get; set; } = "Active";
        public virtual ICollection<Teacher>? Teachers { get; set; }
    }
}
