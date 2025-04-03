using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Helpers;
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

        [HttpGet]
        [Route("Courses")]
        public async Task<IActionResult> IndexCourse(int? pageNumber, string searchString, string sortOrder, int? categoryFilter)
        {
            // เก็บข้อมูลสำหรับ Sorting และ Filtering ลง ViewData
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["StatusSortParam"] = sortOrder == "status" ? "status_desc" : "status";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentCategory"] = categoryFilter;

            // ดึงรายการ Course Categories เพื่อใช้เป็นตัวกรอง
            var categories = await _db.CourseCategories.Where(cc => cc.Status == "Active").ToListAsync();
            ViewData["Categories"] = categories
                .Select(c => new SelectListItem { Value = c.CourseCategoryId.ToString(), Text = c.Name })
                .ToList();

            // Query สำหรับ Courses (รวม CourseCategory เพื่อแสดงชื่อหมวดหมู่)
            var coursesQuery = _db.Course
                .Include(c => c.CourseCategory)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                coursesQuery = coursesQuery.Where(c =>
                    c.CourseName.Contains(searchString) ||
                    c.Course_Code.Contains(searchString) ||
                    (c.Description != null && c.Description.Contains(searchString)));
            }

            // กรองตาม Category Filter หากมีการเลือก
            if (categoryFilter.HasValue)
            {
                coursesQuery = coursesQuery.Where(c => c.CourseCategoryId == categoryFilter.Value);
            }

            coursesQuery = sortOrder switch
            {
                "name_desc" => coursesQuery.OrderByDescending(c => c.CourseName),
                "status" => coursesQuery.OrderBy(c => c.Status),
                "status_desc" => coursesQuery.OrderByDescending(c => c.Status),
                _ => coursesQuery.OrderBy(c => c.CourseName)
            };

            int pageSize = 10;
            var totalItems = await coursesQuery.CountAsync();
            ViewData["TotalItems"] = totalItems;
            var pagedCourses = await PaginatedList<Course>.CreateAsync(coursesQuery, pageNumber ?? 1, pageSize);

            return View(pagedCourses);
        }


        [HttpGet]
        [Route("Courses/Details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            // ดึง Course พร้อมข้อมูล CourseCategory
            var course = await _db.Course
                .Include(c => c.CourseCategory)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
                return NotFound();

            return View(course);
        }


        [HttpGet]
        [Route("Courses/Create")]
        public async Task<IActionResult> CreateCourse()  // Make sure the method name matches your route
        {
            // Convert CourseCategories to IEnumerable<SelectListItem>
            var courseCategories = await _db.CourseCategories
                .Where(cc => cc.Status == "Active")
                .ToListAsync();
            ViewData["CourseCategories"] = courseCategories.Select(c => new SelectListItem
            {
                Value = c.CourseCategoryId.ToString(),
                Text = c.Name
            }).ToList();
            return View("CreateCourse");  // Make sure to return the correct view
        }

        [HttpPost]
        [Route("Courses/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(Course newCourse)
        {
            if (!ModelState.IsValid)
            {
                ViewData["CourseCategories"] = await _db.CourseCategories.ToListAsync();
                return View(newCourse);
            }

            try
            {
                _db.Course.Add(newCourse);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Course created successfully!";
                return RedirectToAction("IndexCourse");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Database Error: {ex.Message}");
            }

            ViewData["CourseCategories"] = await _db.CourseCategories.ToListAsync();
            return View(newCourse);
        }

        [HttpGet]
        [Route("Courses/Edit/{id}")]
        public async Task<IActionResult> EditCourse(int id)
        {
            var course = await _db.Course.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            // Convert CourseCategories to SelectListItems
            var categories = await _db.CourseCategories.Where(cc => cc.Status == "Active").ToListAsync();
            ViewBag.CourseCategories = new SelectList(categories, "CourseCategoryId", "Name");

            return View(course);
        }

        // 📌 บันทึกการแก้ไขคอร์ส
        [HttpPost]
        [Route("Courses/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCourse(Course model)
        {
            var existingCourse = await _db.Course.FindAsync(model.CourseId);
            if (existingCourse == null)
            {
                return NotFound();
            }
            if (!ModelState.IsValid)
            {
                var categories = await _db.CourseCategories.ToListAsync();
                ViewBag.CourseCategories = new SelectList(categories, "CourseCategoryId", "Name");
                return View(model);
            }
            try
            {
                // คงค่า CreatedAt เดิมและอัปเดต UpdatedAt เป็นเวลาปัจจุบัน
                model.CreatedAt = existingCourse.CreatedAt;
                model.UpdatedAt = DateTime.Now;

                // ยกเลิกการ tracking ของ entity เดิมเพื่อหลีกเลี่ยงปัญหาการติดตาม
                _db.Entry(existingCourse).State = EntityState.Detached;

                _db.Course.Update(model);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Course updated successfully!";
                return RedirectToAction("IndexCourse");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating course: {ex.Message}");
                var categories = await _db.CourseCategories.ToListAsync();
                ViewBag.CourseCategories = new SelectList(categories, "CourseCategoryId", "Name");
                return View(model);
            }
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
