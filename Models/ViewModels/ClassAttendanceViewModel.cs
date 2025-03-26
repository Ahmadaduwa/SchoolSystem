using System.Collections.Generic;
using SchoolSystem.Models.ViewModels;

namespace SchoolSystem.ViewModels
{
    public class ClassAttendanceViewModel
    {
        public List<ClassScheduleViewModel> Schedules { get; set; }
        public string CurrentDay { get; set; }
        public string CurrentDate { get; set; }
        public int TeacherId { get; set; }
        public int Debug_AllTeacherSchedulesCount { get; set; }
    }
}