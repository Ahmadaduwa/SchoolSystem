public class EditAttendanceSelectDateViewModel
{
    public int CM_Id { get; set; }
    public string ClassName { get; set; }
    public string CourseName { get; set; }
    public List<DateOnly> CheckedDates { get; set; } = new List<DateOnly>();
}
