using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SchoolSystem.Models.ActivityManagement
{
    public class ActivityAttendanceCheck
    {
        [Key] //เอาไว้รู้ว่าวัน คลาสไหนเช็คชื่อวันไหนไปแล้วบ้าง เพิ่อการค้นหาที่ง่ายขึ้น
        public int ActivityAttendanceCheckId { get; set; }

        [Required]
        public int AM_Id { get; set; }
        [ForeignKey("AM_Id")]
        public virtual ActivityManagement? ActivityManagement { get; set; }
        public DateOnly Date { get; set; }
    }   
}
