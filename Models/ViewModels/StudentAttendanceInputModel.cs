namespace SchoolSystem.ViewModels
{
    public class StudentAttendanceInputModel
    {
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public string? Status { get; set; }
        public bool IsChecked { get; set; } // อาจใช้สำหรับ UI ในการเลือกนักเรียน
    }
}   