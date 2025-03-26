using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolSystem.Models.ClassManagement
{
    public class ClassAttendanceCheck
    {
        [Key] //เอาไว้รู้ว่าวัน คลาสไหนเช็คชื่อวันไหนไปแล้วบ้าง เพิ่อการค้นหาที่ง่ายขึ้น
        public int ClassAttendanceCheckId { get; set; }

        [Required]
        public int CM_Id { get; set; }
        [ForeignKey("CM_Id")]
        public virtual ClassManagement? ClassManagement { get; set; }
        public DateOnly Date { get; set; }
    }   
}
