using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolSystem.Models.ClassManagement;
using SchoolSystem.Models.CourseManagement;
using SchoolSystem.Models.UserManagement;

namespace SchoolSystem.Models.RegistrationManagement
{
    public class RegisteredCourse
    {
        [Key]
        public int RC_id { get; set; }

        [Required(ErrorMessage = "Student ID is required.")]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public Student? Student { get; set; }

        [Required(ErrorMessage = "Course ID is required.")]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public Course? Course { get; set; }

        [Required(ErrorMessage = "Semester ID is required.")]
        public int SemesterId { get; set; }

        [ForeignKey("SemesterId")]
        public Semester? Semester { get; set; }

        [Required(ErrorMessage = "Score is required.")]
        [Range(0, 100, ErrorMessage = "Score must be between 0 and 100.")]
        public float Score { get; set; }

        [Range(0, 4, ErrorMessage = "Grade must be between 0 and 4.")]
        public float Grade { get; set; }

        [Required(ErrorMessage = "Registered Date is required.")]
        [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
        public DateTime RegisteredDate { get; set; }
    }
}
