using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.CouseManagement
{
    public class SubjectCategory
    {
        [Key]
        public int SubjectCategoryId { get; set; } // Primary Key

        [Required]
        [MaxLength(100)] // กำหนดความยาวสูงสุดของชื่อหมวดหมู่
        public string? Name { get; set; }

        [MaxLength(500)] // คำอธิบายหมวดหมู่
        public string? Description { get; set; }
    }
}
