using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SchoolSystem.Data;
using SchoolSystem.Models.CourseManagement;

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

    // จัดเรียงลำดับ
    coursesQuery = sortOrder switch
    {
        "UpdateAt" => coursesQuery.OrderByDescending(c => c.UpdateAt ?? DateTime.MaxValue),
        "CreateAt" => coursesQuery.OrderBy(c => c.CreateAt),
        "Name" => coursesQuery.OrderBy(c => c.CourseName),
        _ => coursesQuery.OrderBy(c => c.Status == "Inactive").ThenByDescending(c => c.UpdateAt ?? DateTime.MaxValue)
    };

    // ดึงข้อมูลไปเป็น List
    var courses = coursesQuery.ToList();

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
        public IActionResult EditCourse(Courses course)
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
        public IActionResult AddCourse(Courses course)
        {
            if (ModelState.IsValid)
            {
                // ตั้งค่าเวลาสร้าง
                course.CreateAt = DateTime.UtcNow;
                course.UpdateAt = DateTime.UtcNow;

                // เพิ่มหลักสูตรใหม่ในฐานข้อมูล
                _db.Courses.Add(course);
                _db.SaveChanges();

                return RedirectToAction("CourseManagement");
            }

            return View(course); // หาก Validation ไม่ผ่าน
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Course/Delete/{id:int}")]
        public IActionResult DeleteCourse(int id)
        {
            var course = _db.Courses.Find(id);
            if (course == null)
            {
                return NotFound(); // หากไม่พบข้อมูล
            }

            _db.Courses.Remove(course); // ลบข้อมูล
            _db.SaveChanges(); // บันทึกการเปลี่ยนแปลง

            return RedirectToAction("CourseManagement"); // กลับไปที่หน้าจัดการหลักสูตร
        }

    }
}
