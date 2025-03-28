namespace SchoolSystem.Models.ViewModels
{
    public class ActivityAttendanceSelectClassViewModel
    {
        public int ActivityManagementId { get; set; }
        public string ActivityName { get; set; } = string.Empty;
        public IEnumerable<ClassManagement.Class> Classes { get; set; } = new List<ClassManagement.Class>();
    }
}
