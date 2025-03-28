using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models.CourseManagement;

namespace SchoolSystem.Controllers
{
    [Authorize(Policy = "AcademicPolicyOrAdminPolicy")]
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
            try
            {
                var courses = _db.Course.Include(c => c.CourseCategory).ToList(); // 🔄 แก้จาก _db.Course → _db.Courses
                return View(courses);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading courses: {ex.Message}";
                return RedirectToAction("IndexCourse");
            }
        }
        public IActionResult CreateCourse()
        {
            ViewBag.CourseCategories = new SelectList(_db.CourseCategories, "CourseCategoryId", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCourse(Course newCourse)
        {
            Console.WriteLine($"CourseCategoryId from Form: {Request.Form["CourseCategoryId"]}");
            Console.WriteLine($"CourseCategoryId from Model: {newCourse.CourseCategoryId}");

            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        Console.WriteLine($"Key: {key}, Error: {error.ErrorMessage}");
                    }
                }

                ViewBag.CourseCategories = new SelectList(_db.CourseCategories, "CourseCategoryId", "Name");
                return View(newCourse);
            }

            try
            {
                _db.Course.Add(newCourse); // 🔹 ใช้ _db.Courses (ไม่ใช่ _db.Course)
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Course created successfully!";
                return RedirectToAction("IndexCourse");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Database Error: {ex.Message}");
            }

            ViewBag.CourseCategories = new SelectList(_db.CourseCategories, "CourseCategoryId", "Name");
            return View(newCourse);
        }




        // 📌 แสดงฟอร์มแก้ไขคอร์ส
        public IActionResult EditCourse(int id)
        {

            try
            {
                var course = _db.Course.Find(id); // 🔄 แก้จาก _db.Course → _db.Courses
                if (course == null)
                {
                    TempData["ErrorMessage"] = "Course not found.";
                    return RedirectToAction("IndexCourse");
                }
                ViewBag.CourseCategories = new SelectList(_db.CourseCategories, "CourseCategoryId", "Name", course.CourseCategoryId);
                return View(course);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading course: {ex.Message}";
                return RedirectToAction("IndexCourse");
            }
        }

        // 📌 บันทึกการแก้ไขคอร์ส
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCourse(Course model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CourseCategories = new SelectList(_db.CourseCategories, "CourseCategoryId", "Name", model.CourseCategoryId);
                return View(model);
            }

            try
            {
                var course = _db.Course.Find(model.CourseId);
                if (course == null)
                {
                    TempData["ErrorMessage"] = "Course not found.";
                    return RedirectToAction("IndexCourse");
                }

                course.Course_Code = model.Course_Code;
                course.CourseName = model.CourseName;
                course.Description = model.Description;
                course.Objective = model.Objective;
                course.Unit = model.Unit;
                course.Status = model.Status;
                course.CourseCategoryId = model.CourseCategoryId;

                _db.SaveChanges();
                TempData["SuccessMessage"] = "Course updated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating course: {ex.Message}";
            }
            return RedirectToAction("IndexCourse");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCourse(int id)
        {
            try
            {
                var course = _db.Course.Include(c => c.ClassManagements)
                                        .Include(c => c.ElectiveCourses)
                                        .Include(c => c.CompulsoryCourses)
                                        .Include(c => c.CompulsoryElectiveCourses)
                                        .FirstOrDefault(c => c.CourseId == id);

                if (course == null)
                {
                    TempData["ErrorMessage"] = "Course not found.";
                    return RedirectToAction("IndexCourse");
                }

                // ตรวจสอบว่าคอร์สนี้ถูกอ้างอิงอยู่หรือไม่
                if (course.ClassManagements.Any() ||
                    course.ElectiveCourses.Any() ||
                    course.CompulsoryCourses.Any() ||
                    course.CompulsoryElectiveCourses.Any())
                {
                    TempData["ErrorMessage"] = "This course cannot be deleted because it is linked to other records.";
                    return RedirectToAction("IndexCourse");
                }

                _db.Course.Remove(course);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Course deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting course: {ex.Message}";
            }

            return RedirectToAction("IndexCourse");
        }

    }
}
