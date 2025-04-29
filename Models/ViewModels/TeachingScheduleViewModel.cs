namespace SchoolSystem.Models.ViewModels
{
    public class TeachingScheduleViewModel
    {
        public string CourseName { get; set; } = string.Empty;
        public string? CourseCode { get; set; }
        public string GradeLevel { get; set; } = string.Empty;
        public int ClassNumber { get; set; }
        public string DayOfWeekEn { get; set; } = string.Empty; // English name from DB
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string SemesterYear { get; set; } = string.Empty;
        public int SemesterNumber { get; set; }
        public string? TeacherName { get; set; }
    }
}