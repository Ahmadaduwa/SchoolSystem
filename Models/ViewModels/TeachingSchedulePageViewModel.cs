namespace SchoolSystem.Models.ViewModels
{
    public class TeachingSchedulePageViewModel
    {
        public int SemesterNumber { get; set; }
        public string? Class { get; set; }
        public string SemesterYear { get; set; } = string.Empty;

        // Set วันที่สอน
        public List<DayViewModel> Days { get; set; } = new()
        {
            new DayViewModel("Monday","จันทร์"),
            new DayViewModel("Tuesday","อังคาร"),
            new DayViewModel("Wednesday","พุธ"),
            new DayViewModel("Thursday","พฤหัสบดี"),
            new DayViewModel("Friday","ศุกร์"),
            //new DayViewModel("Saturday","เสาร์"),
            //new DayViewModel("Sunday","อาทิตย์"),
        };

        // Set คาบเรียน
        public List<(string Name, TimeSpan Start, TimeSpan End)> Periods { get; set; } = new()
        {
            ("คาบ 1", TimeSpan.Parse("08:00"), TimeSpan.Parse("09:20")),
            ("คาบ 2", TimeSpan.Parse("09:20"), TimeSpan.Parse("10:10")),
            ("คาบ 3", TimeSpan.Parse("10:10"), TimeSpan.Parse("11:00")),
            ("คาบ 4", TimeSpan.Parse("11:00"), TimeSpan.Parse("12:00")),
            ("คาบ 5", TimeSpan.Parse("13:00"), TimeSpan.Parse("13:40")),
            ("คาบ 6", TimeSpan.Parse("13:40"), TimeSpan.Parse("14:20")),
            ("คาบ 7", TimeSpan.Parse("14:20"), TimeSpan.Parse("15:00")),
            //("ติวเสริม", TimeSpan.Parse("14:40"), TimeSpan.Parse("16:00")),
        };

        // Matrix indexed by English day name
        public Dictionary<string, Dictionary<string, List<TeachingScheduleViewModel>>> Matrix { get; set; }
            = new Dictionary<string, Dictionary<string, List<TeachingScheduleViewModel>>>();
    }
}