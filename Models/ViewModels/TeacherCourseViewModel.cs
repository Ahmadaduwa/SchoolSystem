using SchoolSystem.Models.ViewModels;

namespace SchoolSystem.ViewModels
{
    public class TeacherCourseViewModel
    {
        public int CM_Id { get; set; }
        public int TeacherId { get; set; } // รหัสครู
        public string? TeacherName { get; set; } // ชื่อครู
        public int CourseId { get; set; } // รหัสวิชา
        public string? CourseName { get; set; } // ชื่อวิชา
        public string? CourseCode { get; set; } // รหัสวิชา
        public int ClassId { get; set; } // รหัสห้องเรียน
        public int ClassNumber { get; set; } // หมายเลขห้องเรียน
        public string ClassName { get; set; }
        public int SemesterId { get; set; } // ภาคการศึกษา
        public string SemesterName { get; set; } // ชื่อภาคการศึกษา
        public List<ClassScheduleViewModel> Schedules { get; set; } = new(); // ตารางสอนของครูในรายวิชานี้
    }
}
