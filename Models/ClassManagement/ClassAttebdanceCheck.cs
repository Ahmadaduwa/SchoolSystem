using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.ClassManagement
{
    public class ClassAttebdanceCheck
    {
        [Key] //เอาไว้รู้ว่าวัน คลาสไหนเช็คชื่อวันไหนไปแล้วบ้าง เพิ่อการค้นหาที่ง่ายขึ้น
        public int ClassAttebdanceCheckId { get; set; }
        public int CM_Id { get; set; }
        public DateOnly Date { get; set; }
    }   
}
