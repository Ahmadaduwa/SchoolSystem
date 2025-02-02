using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.ClassManagement
{
    public class ClassAttendance
    {
        [Key] //เอาไว้เช็คชื่อ
        public int ClassAttendanceId { get; set; }
        public int CM_Id { get; set; }
        public int StudentId { get; set; }

        public String? Status { get; set; }

        public DateOnly Date { get; set; }
    }
}
