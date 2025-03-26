using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Helpers;
using SchoolSystem.Models.ClassManagement;

namespace SchoolSystem.Controllers
{
    [Authorize(Policy = "AdminPolicy")]
    public class GradeLevelController : Controller
    {
        private readonly AppDbContext _db;

        public GradeLevelController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Route("GradeLevels")]
        public async Task<IActionResult> Index(int? pageNumber, string searchString, string sortOrder, string statusFilter)
        {
            // เก็บข้อมูลสำหรับ sorting และ filtering ลง ViewData
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["StatusSortParam"] = sortOrder == "status" ? "status_desc" : "status";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = statusFilter;

            // สร้างรายการสถานะเพื่อใช้เป็นตัวกรอง (ถ้ามีข้อมูลสถานะใน GradeLevels)
            var statuses = await _db.GradeLevels
                .Select(g => g.Status)
                .Distinct()
                .ToListAsync();
            ViewData["Statuses"] = statuses
                .Select(s => new SelectListItem { Value = s, Text = s })
                .ToList();

            // สร้าง query เริ่มต้น
            var gradeLevelsQuery = _db.GradeLevels.AsNoTracking();

            // ค้นหาตามชื่อ หรือคำใน Description
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                gradeLevelsQuery = gradeLevelsQuery.Where(g =>
                    g.Name.Contains(searchString) ||
                    (g.Description != null && g.Description.Contains(searchString)));
            }

            // กรองตามสถานะ (Status)
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                gradeLevelsQuery = gradeLevelsQuery.Where(g => g.Status == statusFilter);
            }

            // จัดเรียงข้อมูล
            gradeLevelsQuery = sortOrder switch
            {
                "name_desc" => gradeLevelsQuery.OrderByDescending(g => g.Name),
                "status" => gradeLevelsQuery.OrderBy(g => g.Status),
                "status_desc" => gradeLevelsQuery.OrderByDescending(g => g.Status),
                _ => gradeLevelsQuery.OrderBy(g => g.Name),
            };

            // แบ่งหน้า
            int pageSize = 10;
            var totalItems = await gradeLevelsQuery.CountAsync();
            ViewData["TotalItems"] = totalItems;
            var pagedGradeLevels = await PaginatedList<GradeLevels>.CreateAsync(gradeLevelsQuery, pageNumber ?? 1, pageSize);

            return View(pagedGradeLevels);
        }

        [HttpGet]
        [Route("GradeLevels/Details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var gradeLevel = await _db.GradeLevels
                .FirstOrDefaultAsync(m => m.GradeLevelId == id);

            if (gradeLevel == null)
                return NotFound();

            return View(gradeLevel);
        }

        [HttpGet]
        [Route("GradeLevels/Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("GradeLevels/Create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(GradeLevels newGradeLevel)
        {
            if (!ModelState.IsValid)
            {
                return View(newGradeLevel);
            }

            try
            {
                _db.GradeLevels.Add(newGradeLevel);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Grade level created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Database Error: {ex.Message}");
            }

            return View(newGradeLevel);
        }

        // แสดงหน้าแก้ไข GradeLevel
        [HttpGet]
        [Route("GradeLevels/Edit/{id}")]
        public IActionResult Edit(int id)
        {
            var gradeLevel = _db.GradeLevels.Find(id);
            if (gradeLevel == null)
            {
                return NotFound();
            }
            return View(gradeLevel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("GradeLevels/Edit/{id}")]
        public IActionResult Edit(GradeLevels updatedGradeLevel)
        {
            // Find the existing grade level in the database
            var existingGradeLevel = _db.GradeLevels.Find(updatedGradeLevel.GradeLevelId);

            if (existingGradeLevel == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(updatedGradeLevel);
            }

            try
            {
                // Preserve the original CreatedAt timestamp
                updatedGradeLevel.CreatedAt = existingGradeLevel.CreatedAt;

                // Update the UpdatedAt timestamp to current time
                updatedGradeLevel.UpdatedAt = DateTime.Now;

                // Detach the existing entity to avoid tracking conflicts
                _db.Entry(existingGradeLevel).State = EntityState.Detached;

                // Update the entire entity
                _db.GradeLevels.Update(updatedGradeLevel);
                _db.SaveChanges();

                TempData["SuccessMessage"] = "Grade level updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Log the exception if possible
                ModelState.AddModelError("", $"Error updating grade level: {ex.Message}");
                return View(updatedGradeLevel);
            }
        }

        [HttpPost]
        [Route("GradeLevels/Delete/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminPolicy")] 
        public async Task<IActionResult> Delete(int id)
        {
            try
            {   
                var gradeLevel = await _db.GradeLevels
                    .FirstOrDefaultAsync(gl => gl.GradeLevelId == id);

                if (gradeLevel == null)
                {
                    TempData["ErrorMessage"] = "Grade level not found.";
                    return RedirectToAction("Index");
                }

                // Check for related records before deleting
                bool hasRelatedClasses = await _db.Classes
                    .AnyAsync(c => c.GradeLevelId == id);

                if (hasRelatedClasses)
                {
                    TempData["ErrorMessage"] = "Cannot delete grade level with existing classes.";
                    return RedirectToAction("Index");
                }

                _db.GradeLevels.Remove(gradeLevel);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Grade level deleted successfully!";
            }
            catch (Exception ex)
            {
                // Consider logging the exception
                TempData["ErrorMessage"] = $"Error deleting grade level: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
