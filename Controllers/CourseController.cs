using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models.CourseManagement;

namespace SchoolSystem.Controllers
{
    public class CourseController : Controller
    {
        private readonly AppDbContext _db;

        public CourseController(AppDbContext db)
        {
            _db = db;
        }

        // 📌 แสดงรายการคอร์สทั้งหมด
        public IActionResult IndexCourse()
        {
            var activeCourses = _db.Course.Where(c => c.Status == "Active").ToList();
            return View(activeCourses);
        }

        // 📌 สร้างคอร์สใหม่
        public IActionResult CreateCourse()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCourse(Course obj)
        {
            if (ModelState.IsValid)
            {
                _db.Course.Add(obj);
                try
                {
                    _db.SaveChanges();
                    TempData["SuccessMessage"] = "Course created successfully!";
                    return RedirectToAction("IndexCourse");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }
            return View(obj);
        }

        // 📌 แก้ไขคอร์ส
        public IActionResult EditCourse(int id)
        {
            var course = _db.Course.Find(id);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Course not found.";
                return RedirectToAction("IndexCourse");
            }
            return View(course);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCourse(Course obj)
        {
            if (ModelState.IsValid)
            {
                var courseToUpdate = _db.Course.Find(obj.CourseId);
                if (courseToUpdate != null)
                {
                    courseToUpdate.Course_Code = obj.Course_Code;
                    courseToUpdate.CourseName = obj.CourseName;
                    courseToUpdate.Objective = obj.Objective;
                    courseToUpdate.Unit = obj.Unit;
                    courseToUpdate.CourseCategoryId = obj.CourseCategoryId;
                    courseToUpdate.Status = obj.Status;

                    _db.SaveChanges();
                    TempData["SuccessMessage"] = "Course updated successfully!";
                    return RedirectToAction("IndexCourse");
                }
            }

            TempData["ErrorMessage"] = "Course not found.";
            return View(obj);
        }

        // 📌 ลบคอร์สจากหน้า Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCourse(int id)
        {
            try
            {
                var obj = _db.Course.Find(id);
                if (obj != null)
                {
                    _db.Course.Remove(obj);
                    _db.SaveChanges();
                    TempData["SuccessMessage"] = "Course deleted successfully!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Course not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting course: {ex.Message}";
            }
            return RedirectToAction("IndexCourse");
        }
    }
}
