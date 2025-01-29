using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models.CourseManagement;
using SchoolSystem.Models.ViewModels;


namespace SchoolSystem.Controllers
{
    [Authorize(Policy = "AcademicPolicy")]
    public class AcademicController : Controller
    {
        private readonly AppDbContext _db;

        public AcademicController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Route("Course")]
        public IActionResult CourseManagement(string sortOrder, string filterStatus)
        {
            // เริ่มต้น Query
            var coursesQuery = _db.Courses.AsQueryable();

            // กรองสถานะ (Active/Inactive)
            if (!string.IsNullOrEmpty(filterStatus))
            {
                coursesQuery = coursesQuery.Where(c => c.Status == filterStatus);
            }

            coursesQuery = sortOrder switch
            {
                "UpdateAtDesc" => coursesQuery.OrderByDescending(c => c.UpdateAt ?? DateTime.MaxValue),
                "UpdateAtAsc" => coursesQuery.OrderBy(c => c.UpdateAt ?? DateTime.MaxValue),
                "CreateAtDesc" => coursesQuery.OrderByDescending(c => c.CreateAt),
                "CreateAtAsc" => coursesQuery.OrderBy(c => c.CreateAt),
                "NameAsc" => coursesQuery.OrderBy(c => c.CourseName),
                "NameDesc" => coursesQuery.OrderByDescending(c => c.CourseName),
                _ => coursesQuery.OrderBy(c => c.Status == "Inactive").ThenByDescending(c => c.UpdateAt ?? DateTime.MaxValue)
            };


            List<Course> courses = coursesQuery.ToList();

            // ส่งข้อมูลไปยัง View
            return View(courses);
        }


        [HttpGet]
        [Route("Course/Edit/{id:int}")] // เพิ่มการกำหนดประเภทพารามิเตอร์
        public IActionResult EditCourse(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }
            var obj = _db.Courses.Find(id);
            if (obj == null)
            {
                return NotFound(); // หากไม่พบ Course
            }
            return View(obj);
        }


        [HttpPost]
        [Route("Course/Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult EditCourse(Course course)
        {
            try
            {
                if (ModelState.IsValid)
                { // ดึงข้อมูลเดิมจากฐานข้อมูล
                    var existingCourse = _db.Courses.Find(course.CourseId);
                    if (existingCourse == null)
                    {
                        return NotFound(); // หากไม่พบหลักสูตรในฐานข้อมูล
                    }

                    // เก็บค่าที่ไม่ควรเปลี่ยนแปลง
                    course.CreateAt = existingCourse.CreateAt;

                    // อัปเดตเวลาล่าสุด
                    course.UpdateAt = DateTime.UtcNow;

                    // อัปเดตข้อมูล
                    _db.Entry(existingCourse).CurrentValues.SetValues(course);
                    _db.SaveChanges();

                    return RedirectToAction("CourseManagement");
                }
            }
            catch (Exception ex)
            {
                // Log ข้อผิดพลาด
                Console.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError("", "เกิดข้อผิดพลาดในการบันทึกข้อมูล");
            }

            return View(course);
        }

        [HttpGet]
        [Route("Course/Add")]
        public IActionResult AddCourse()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Course/Add")]
        public IActionResult AddCourse(Course course)
        {
            if (ModelState.IsValid)
            {
                course.CreateAt = DateTime.UtcNow;
                course.UpdateAt = DateTime.UtcNow;

                _db.Courses.Add(course);
                _db.SaveChanges();

                return RedirectToAction("CourseManagement");
            }

            return View(course); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Course/Delete/{id:int}")]
        public IActionResult DeleteCourse(int id)
        {
            var course = _db.Courses.Find(id);
            if (course == null)
            {
                return NotFound(); 
            }

            _db.Courses.Remove(course);
            _db.SaveChanges(); 

            return RedirectToAction("CourseManagement"); 
        }

        [HttpGet]
        [Route("Course/Activity/{id:int}")]
        public IActionResult CourseActivity(int id)
        {
            var course = _db.Courses.FirstOrDefault(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            var extracurricularActivities = _db.ExtracurricularActivities
                .Where(ea => ea.CourseId == id)
                .Include(ea => ea.Activity) // โหลดข้อมูล Activity
                .ToList(); // ดึงข้อมูลทั้งหมดก่อน

            var activities = extracurricularActivities
                .Select(ea => ea.Activity)
                .Where(a => a != null)
                .ToList(); // แปลงเป็น List<Activity> และป้องกัน null

            var courseActivityViewModel = new CourseActivityViewModel
            {
                CourseId = course.CourseId,
                Course_Code = course.Course_Code,
                CourseName = course.CourseName,
                Activities = activities // กำหนดให้ ViewModel
            };



            return View(courseActivityViewModel);
        }   



    }
}
