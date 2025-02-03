using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        // 📌 แสดงฟอร์มสร้างคอร์ส
        public IActionResult CreateCourse()
        {
            try
            {
                var categories = _db.CourseCategories.ToList();

                if (categories == null || categories.Count == 0)
                {
                    TempData["ErrorMessage"] = "No course categories found. Please add categories first.";
                    return RedirectToAction("IndexCourse");
                }

                ViewBag.CourseCategories = new SelectList(categories, "CourseCategoryId", "Name");
                return View();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading categories: {ex.Message}";
                return RedirectToAction("IndexCourse");
            }
        }


        // 📌 บันทึกการสร้างคอร์ส
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCourse(Course model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CourseCategories = new SelectList(_db.CourseCategories, "CourseCategoryId", "Name");
                return View(model);
            }

            try
            {
                _db.Course.Add(model);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Course created successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating course: {ex.Message}";
            }
            return RedirectToAction("IndexCourse");
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
    }
}
