using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models.CourseManagement;

namespace SchoolSystem.Controllers
{
    public class CategoryController : Controller
    {
        private readonly AppDbContext _db;

        public CategoryController(AppDbContext db)
        {
            _db = db;
        }

        // 📌 แสดงรายการหมวดหมู่คอร์สทั้งหมด
        public IActionResult IndexCategory()
        {
            try
            {
                var categories = _db.CourseCategories.ToList();
                return View(categories);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading categories: {ex.Message}";
                return View(new List<CourseCategory>());
            }
        }

        // 📌 แสดงฟอร์มสร้างหมวดหมู่คอร์ส
        public IActionResult CreateCategory()
        {
            return View();
        }

        // 📌 บันทึกการสร้างหมวดหมู่คอร์ส
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCategory(CourseCategory model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _db.CourseCategories.Add(model);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Subject category created successfully!";
                return RedirectToAction("IndexCategory");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating category: {ex.Message}");
                return View(model);
            }
        }

        // 📌 แสดงฟอร์มแก้ไขหมวดหมู่คอร์ส
        public IActionResult EditCategory(int id)
        {
            try
            {
                var category = _db.CourseCategories.Find(id);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Category not found.";
                    return RedirectToAction("IndexCategory");
                }
                return View(category);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading category: {ex.Message}";
                return RedirectToAction("IndexCategory");
            }
        }

        // 📌 บันทึกการแก้ไขหมวดหมู่คอร์ส
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCategory(CourseCategory model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var category = _db.CourseCategories.Find(model.CourseCategoryId);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Category not found.";
                    return RedirectToAction("IndexCategory");
                }

                category.Name = model.Name;
                category.Description = model.Description;

                _db.SaveChanges();
                TempData["SuccessMessage"] = "Subject category updated successfully!";
                return RedirectToAction("IndexCategory");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating category: {ex.Message}");
                return View(model);
            }
        }

        // 📌 ลบหมวดหมู่คอร์สจากหน้า Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCategory(int id)
        {
            try
            {
                var category = _db.CourseCategories.Include(c => c.Courses).FirstOrDefault(c => c.CourseCategoryId == id);
                if (category == null)
                {
                    TempData["ErrorMessage"] = "Category not found.";
                    return RedirectToAction("IndexCategory");
                }

                // ตรวจสอบว่าหมวดหมู่มีคอร์สที่เชื่อมโยงอยู่หรือไม่
                if (category.Courses.Any())
                {
                    TempData["ErrorMessage"] = "Cannot delete category because it is associated with existing courses.";
                    return RedirectToAction("IndexCategory");
                }

                _db.CourseCategories.Remove(category);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Subject category deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting category: {ex.Message}";
            }
            return RedirectToAction("IndexCategory");
        }
    }
}
