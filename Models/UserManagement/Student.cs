using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolSystem.Models.ActivityManagement;

namespace SchoolSystem.Models.UserManagement
{
    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        [Required]
        [StringLength(50)]
        public string? Student_Code { get; set; }

        public int? ProfileId { get; set; } 

        [ForeignKey("ProfileId")]
        public Profiles Profile { get; set; } = new Profiles();

        [Required]
        public int ClassId { get; set; }

        public DateOnly EnrollmentDate { get; set; }

        public int GuardianId { get; set; }

        public float GPA { get; set; }

        public string? Status { get; set; } = "Active";
        public DateTime UpdateAt { get; set; } = DateTime.UtcNow;

        public DateTime CreateAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<ActivityAttendance> ActivityAttendance { get; set; } = new List<ActivityAttendance>();
    }
}
