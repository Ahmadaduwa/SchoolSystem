using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolSystem.Data;
using SchoolSystem.Models.ClassManagement;
using SchoolSystem.Models.ViewModels;

namespace SchoolSystem.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly AppDbContext _db;

        public AttendanceController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Route("ClassAttendance")]
        [Authorize(Policy = "TeacherPolicy")]
        public IActionResult ClassAttendance()
        {
            var subjects = new List<dynamic>
            {
                new { Id = 1, Name = "คณิตศาสตร์", Time = "08:00 น." },
                new { Id = 2, Name = "วิทยาศาสตร์", Time = "09:30 น." },
                new { Id = 3, Name = "ภาษาอังกฤษ", Time = "10:45 น." },
                new { Id = 4, Name = "ฟิสิกส์", Time = "13:00 น." },
                new { Id = 5, Name = "เคมี", Time = "14:30 น." },
                new { Id = 6, Name = "ชีววิทยา", Time = "16:00 น." },
                new { Id = 7, Name = "สังคมศึกษา", Time = "17:30 น." },
                new { Id = 8, Name = "ประวัติศาสตร์", Time = "18:45 น." },
                new { Id = 9, Name = "คอมพิวเตอร์", Time = "20:00 น." },
                new { Id = 10, Name = "ศิลปะ", Time = "21:15 น." }
            };

            return View(subjects);
        }
        [HttpGet]
        [Route("ClassAttendance/check")]
        [Authorize(Policy = "TeacherPolicy")]
        public IActionResult StudentAttendance()
        {
            var students = Enumerable.Range(1, 45).Select(i => new StudentCheckViewModel
            {
                Id = i,
                StudentId = $"STU{i:D3}",
                FirstName = $"ชื่อ{i}",
                LastName = $"นามสกุล{i}",
                IsChecked = false
            }).ToList();

            ViewData["Subject"] = "คณิตศาสตร์";
            ViewData["ClassRoom"] = "ห้อง 101";
            return View(students);
        }

        [HttpGet]
        [Route("ClassAttendance/AllSubjects")]
        [Authorize(Policy = "TeacherPolicy")]
        public IActionResult AllSubjects()
        {
            // จำลองข้อมูลวิชาที่อาจารย์สอน
            var subjects = new List<dynamic>
            {
                new { Id = 1, Name = "คณิตศาสตร์", Time = "08:00 - 09:30" },
                new { Id = 2, Name = "ฟิสิกส์", Time = "09:45 - 11:15" },
                new { Id = 3, Name = "เคมี", Time = "13:00 - 14:30" }
            };
            return View(subjects);
        }

        [HttpGet]
        [Route("ClassAttendance/EditAttendance")]
        [Authorize(Policy = "TeacherPolicy")]
        public IActionResult EditAttendance(string? searchDate)
        {
            DateTime selectedDate;

            if (!string.IsNullOrEmpty(searchDate) && DateTime.TryParse(searchDate, out selectedDate))
            {
                ViewData["Date"] = selectedDate.ToString("yyyy-MM-dd"); // แก้ให้แสดงค่าวันที่ตรงกับ input date
            }
            else
            {
                selectedDate = DateTime.Now;
                ViewData["Date"] = selectedDate.ToString("yyyy-MM-dd"); // Format ให้เข้ากับ input date
            }

            // ค้นหาข้อมูลนักเรียนที่เคยถูกเช็คชื่อในวันนั้น (หรือดึงจาก Database จริง)
            var students = Enumerable.Range(1, 45).Select(i => new StudentCheckViewModel
            {
                Id = i,
                StudentId = $"STU{i:D3}",
                FirstName = $"ชื่อ{i}",
                LastName = $"นามสกุล{i}",
                Status = "ยังไม่เช็คชื่อ" // ค่าเริ่มต้นของสถานะ
            }).ToList();

            ViewData["Subject"] = "คณิตศาสตร์";
            ViewData["ClassRoom"] = "ห้อง 101";

            return View(students);
        }


    }
}
