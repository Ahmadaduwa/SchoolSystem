using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolSystem.Models.ActivityManagement;
using SchoolSystem.Models.Assignment;
using SchoolSystem.Models.ClassManagement;
using SchoolSystem.Models.RegistrationManagement;

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
        public Profiles? Profile { get; set; }

        [Required]
        public int ClassId { get; set; }
        [ForeignKey("ClassId")]
        public Class? Class { get; set; }

        public DateOnly EnrollmentDate { get; set; }

        public float GPA { get; set; }

        public string? Status { get; set; } = "Active";
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<ActivityAttendance> ActivityAttendance { get; set; } = new List<ActivityAttendance>();
        public virtual ICollection<ClassAttendance> ClassAttendance { get; set; } = new List<ClassAttendance>();
        public virtual ICollection<ClassAttendanceSummary> ClassAttendanceSummary { get; set; } = new List<ClassAttendanceSummary>();
        public virtual ICollection<ActivityAttendanceSummary> AcitivityAttendanceSummary { get; set; } = new List<ActivityAttendanceSummary>();
        public virtual ICollection<RegisteredCourse> RegisteredCourse { get; set; } = new List<RegisteredCourse>();
        public virtual ICollection<AssignmentScore> AssignmentScores { get; set; } = new List<AssignmentScore>();
    }
}
