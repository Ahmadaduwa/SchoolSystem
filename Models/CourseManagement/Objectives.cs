using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.CourseManagement
{
    public class Objectives
    {
        [Key]
        public int ObjectiveID { get; set; } // Primary Key

        [Required]
        [StringLength(100)] // จำกัดความยาวสูงสุดของชื่อวัตถุประสงค์
        public required string ObjectiveName { get; set; }

        [StringLength(500)] // จำกัดความยาวสูงสุดของคำอธิบาย
        public string? Description { get; set; }
    }
}
