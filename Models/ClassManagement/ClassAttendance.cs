using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SchoolSystem.Models.UserManagement;

namespace SchoolSystem.Models.ClassManagement
{
    public class ClassAttendance
    {
        [Key] //เอาไว้เช็คชื่อ
        public int ClassAttendanceId { get; set; }

        [Required]
        [ForeignKey("CM_Id")]
        public int CM_Id { get; set; }
        public virtual ClassManagement? ClassManagement { get; set; }

        [Required]
        [ForeignKey("StudentId")]
        public int StudentId { get; set; }
        public virtual Student? Student { get; set; }

        public String? Status { get; set; }

        public DateOnly Date { get; set; }
    }
}
