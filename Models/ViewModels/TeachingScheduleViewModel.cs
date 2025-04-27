namespace SchoolSystem.Models.ViewModels
{
    public class TeachingScheduleViewModel
    {
        public string CourseName { get; set; } = string.Empty;
        public int ClassNumber { get; set; }
        public string GradeLevel { get; set; } = string.Empty;
        public string DayOfWeek { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string? SemesterYear { get; set; }
        public int SemesterNumber { get; set; }
    }
}