namespace SchoolSystem.Models.ClassManagement
{
    public class StatusAttendanceSummary
    {
        public int SAM_Id { get; set; }
        public int CM_Id { get; set; }
        public int StudentId { get; set; }
        public int PresentCount { get; set; } //มา
        public int AbsentCount { get; set; } //ไม่มา
        public int LateCount { get; set; } //มาสาย
        public int ExcusedCount { get; set; } //ลา
        
        public DateTime UpdateAt { get; set; }
    }
}
