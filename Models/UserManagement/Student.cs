using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.UserManagement
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        [Required]
        public string? Student_Code { get; set; }

        [Required]
        public string? UserId { get; set; }

        [Required]
        public int ClassId { get; set; }

        public DateOnly EnrollmentDate { get; set; }

        public int GuardianId { get; set; }

        public float GPA { get; set; }

        public string? Status { get; set; } = "Active";
        public DateTime UpdateAt { get; set; } = DateTime.UtcNow;

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;


    }
}
