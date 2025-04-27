using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Helpers;
using SchoolSystem.Models.CourseManagement;

namespace SchoolSystem.Controllers
{
    [Authorize(Policy = "AdminPolicy")]
    public class CategoryController : Controller
    {
        private readonly AppDbContext _db;

        public CategoryController(AppDbContext db)
        {
            _db = db;
        }

        // 📌 แสดงรายการหมวดหมู่คอร์สทั้งหมด
        [HttpGet]
        [Route("CourseCategories")]
        public async Task<IActionResult> IndexCategory(int? pageNumber, string searchString, string sortOrder, string statusFilter)
        {
            // เก็บข้อมูลสำหรับ sorting และ filtering ลง ViewData
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["StatusSortParam"] = sortOrder == "status" ? "status_desc" : "status";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = statusFilter;

            // สร้างรายการสถานะสำหรับการกรอง โดยดึงข้อมูลจาก CourseCategories
            var statuses = await _db.CourseCategories
                .Select(c => c.Status)
                .Distinct()
                .ToListAsync();
            ViewData["Statuses"] = statuses
                .Select(s => new SelectListItem { Value = s, Text = s })
                .ToList();

            // สร้าง query เริ่มต้นสำหรับ CourseCategories
            var courseCategoriesQuery = _db.CourseCategories.AsNoTracking();

            // ค้นหาจาก Name และ Description
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                courseCategoriesQuery = courseCategoriesQuery.Where(c =>
                    c.Name.Contains(searchString) ||
                    (c.Description != null && c.Description.Contains(searchString)));
            }

            // กรองตามสถานะ (Status)
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                courseCategoriesQuery = courseCategoriesQuery.Where(c => c.Status == statusFilter);
            }

            // จัดเรียงข้อมูลตาม sortOrder
            courseCategoriesQuery = sortOrder switch
            {
                "name_desc" => courseCategoriesQuery.OrderByDescending(c => c.Name),
                "status" => courseCategoriesQuery.OrderBy(c => c.Status),
                "status_desc" => courseCategoriesQuery.OrderByDescending(c => c.Status),
                _ => courseCategoriesQuery.OrderBy(c => c.Name),
            };

            // กำหนดการแบ่งหน้า
            int pageSize = 10;
            var totalItems = await courseCategoriesQuery.CountAsync();
            ViewData["TotalItems"] = totalItems;
            var pagedCourseCategories = await PaginatedList<CourseCategory>.CreateAsync(courseCategoriesQuery, pageNumber ?? 1, pageSize);

            return View(pagedCourseCategories);
        }


        // 📌 แสดงฟอร์มสร้างหมวดหมู่คอร์ส
        [HttpGet]
        [Route("CourseCategories/Create")]
        public IActionResult CreateCategory()
        {
            return View();
        }

        // 📌 บันทึกการสร้างหมวดหมู่คอร์ส
        [HttpPost]
        [Route("CourseCategories/Create")]
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
                TempData["SuccessMessage"] = "Course category created successfully!";
                return RedirectToAction("IndexCategory");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Database Error: {ex.Message}");
            }

            return View(model);
        }

        // 📌 แสดงฟอร์มแก้ไขหมวดหมู่คอร์ส
        [HttpGet]
        [Route("CourseCategories/Edit/{id}")]
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
        [Route("CourseCategories/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult EditCategory(CourseCategory model)
        {
            // ค้นหาข้อมูล CourseCategory ที่มีอยู่ในฐานข้อมูล
            var existingCategory = _db.CourseCategories.Find(model.CourseCategoryId);

            if (existingCategory == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // คงค่า CreatedAt เดิมไว้
                model.CreatedAt = existingCategory.CreatedAt;

                // อัปเดตค่า UpdatedAt ให้เป็นเวลาปัจจุบัน
                model.UpdatedAt = DateTime.Now;

                // ยกเลิกการ tracking ของ entity เดิมเพื่อหลีกเลี่ยงปัญหาการติดตามหลายตัว
                _db.Entry(existingCategory).State = EntityState.Detached;

                // อัปเดต entity ใหม่ทั้งหมด
                _db.CourseCategories.Update(model);
                _db.SaveChanges();

                TempData["SuccessMessage"] = "Course category updated successfully!";
                return RedirectToAction("IndexCategory");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating course category: {ex.Message}");
                return View(model);
            }
        }

        // 📌 ลบหมวดหมู่คอร์สจากหน้า Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminPolicy")]
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
