public class AttendanceSummaryViewModel
{
    public string StudentCode { get; set; }
    public string Name { get; set; }
    public int PresentCount { get; set; }
    public int AbsentCount { get; set; }
    public int LateCount { get; set; }
    public int ExcusedCount { get; set; }
    public float Percent { get; set; }
}
