using SchoolSystem.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace SchoolSystem.Models.ViewModels
{
    public class ActivityAttendanceViewModel
    {
        public int ActivityManagementId { get; set; }
        [Required]
        public string? ActivityName { get; set; }
        public DateTime AttendanceDate { get; set; }
        public int ClassId { get; set; }

        public DateTime CurrentDate { get; set; }
        public string? CurrentDay { get; set; }

        public List<ScheduleViewModel>? Schedules { get; set; }
        public List<StudentAttendanceInputModel>? Students { get; set; }
    }

    public class ScheduleViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
