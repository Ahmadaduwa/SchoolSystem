namespace SchoolSystem.Models.ViewModels
{
    public class ClassScheduleViewModel
    {
        public int Id { get; set; }                      // รหัส ClassManagement (CM_Id)
        public string Name { get; set; } = string.Empty; // ชื่อวิชา (CourseName)
        public string Time { get; set; } = string.Empty; // เวลาเริ่มเรียน
        public int ClassId { get; set; }                 // รหัส Class
        public int CourseId { get; set; }                // รหัสวิชา
        public int SemesterId { get; set; }              // รหัสเทอม
        public int TeacherId { get; set; }               // รหัสครู


        // เพิ่มข้อมูลที่เป็นประโยชน์
        public int ClassNumber { get; set; }             // หมายเลขห้องเรียน
        public string GradeLevel { get; set; } = string.Empty; // ระดับชั้น
        public string CourseCode { get; set; } = string.Empty; // รหัสวิชา

        // คุณสมบัติเพิ่มเติมที่อาจเป็นประโยชน์ในการแสดงผล
        public string DisplayName => $"{CourseCode} - {Name}";
        public string DisplayClass => $"ม.{GradeLevel}/{ClassNumber}";
    }
}