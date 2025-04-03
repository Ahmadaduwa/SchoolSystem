using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Helpers;
using SchoolSystem.Models.ClassManagement;

namespace SchoolSystem.Controllers
{
    [Authorize(Policy = "AcademicPolicyOrAdminPolicy")]
    public class ClassController : Controller
    {
        private readonly AppDbContext _db;

        public ClassController(AppDbContext db)
        {
            _db = db;
        }
        [HttpGet]
        [Route("Classes")]
        public async Task<IActionResult> IndexClass(int? pageNumber, string searchString, string gradeLevelFilter)
        {
            // เก็บข้อมูลการกรองที่ใช้ค้นหาไว้ใน ViewData
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentGradeLevel"] = gradeLevelFilter;

            // Populate GradeLevel filter dropdown
            var gradeLevels = await _db.GradeLevels
                .Where(g => g.Status == "Active")
                .Select(g => g.Name)
                .Distinct()
                .ToListAsync();
            ViewData["GradeLevels"] = gradeLevels
                .Select(g => new SelectListItem { Value = g, Text = g })
                .ToList();

            // สร้าง base query พร้อม Include GradeLevels
            var classesQuery = _db.Classes
                .Include(c => c.GradeLevels)
                .AsNoTracking();

            // กรองจากคำค้นหา (ค้นหาจาก ClassNumber หรือ GradeLevel.Name)
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                classesQuery = classesQuery.Where(c =>
                    (c.GradeLevels.Name + "/" + c.ClassNumber.ToString()).Contains(searchString));
            }


            // กรองตาม GradeLevel ที่เลือก (ถ้ามี)
            if (!string.IsNullOrWhiteSpace(gradeLevelFilter))
            {
                classesQuery = classesQuery.Where(c => c.GradeLevels.Name == gradeLevelFilter);
            }

            // เรียงลำดับข้อมูล โดยเรียงตาม GradeLevel.Name แล้วตามด้วย ClassNumber
            classesQuery = classesQuery
                .OrderBy(c => c.GradeLevels.Name)
                .ThenBy(c => c.ClassNumber);

            // การแบ่งหน้า
            int pageSize = 10;
            var totalItems = await classesQuery.CountAsync();
            ViewData["TotalItems"] = totalItems;
            var pagedClasses = await PaginatedList<Class>.CreateAsync(classesQuery, pageNumber ?? 1, pageSize);

            return View(pagedClasses);
        }


        [HttpGet]
        [Route("Classes/Details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var classDetails = await _db.Classes
                .Include(c => c.GradeLevels)
                .FirstOrDefaultAsync(m => m.ClassId == id);

            if (classDetails == null)
                return NotFound();

            return View(classDetails);
        }

        [HttpGet]
        [Route("Classes/Create")]
        public IActionResult CreateClass()
        {
            // Populate GradeLevel dropdown
            ViewBag.GradeLevels = _db.GradeLevels
                .Where(g => g.Status == "Active")
                .Select(g => new SelectListItem
                {
                    Value = g.GradeLevelId.ToString(),
                    Text = g.Name
                })
                .ToList();
            return View();
        }

        [HttpPost]
        [Route("Classes/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateClass(Class newClass)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate GradeLevel dropdown
                ViewBag.GradeLevels = _db.GradeLevels
                    .Select(g => new SelectListItem
                    {
                        Value = g.GradeLevelId.ToString(),
                        Text = g.Name
                    })
                    .ToList();
                return View(newClass);
            }

            try
            {
                // Validate GradeLevel exists
                var gradeLevel = await _db.GradeLevels
                    .FirstOrDefaultAsync(g => g.GradeLevelId == newClass.GradeLevelId);

                if (gradeLevel == null)
                {
                    ModelState.AddModelError("GradeLevelId", "Invalid Grade Level selected.");
                    ViewBag.GradeLevels = _db.GradeLevels
                        .Select(g => new SelectListItem
                        {
                            Value = g.GradeLevelId.ToString(),
                            Text = g.Name
                        })
                        .ToList();
                    return View(newClass);
                }

                // Check for duplicate class name within the same grade level
                var existingClass = await _db.Classes
                    .FirstOrDefaultAsync(c =>
                        c.ClassNumber == newClass.ClassNumber &&
                        c.GradeLevelId == newClass.GradeLevelId);

                if (existingClass != null)
                {
                    ModelState.AddModelError("ClassName", "A class with this name already exists in the selected grade level.");
                    ViewBag.GradeLevels = _db.GradeLevels
                        .Select(g => new SelectListItem
                        {
                            Value = g.GradeLevelId.ToString(),
                            Text = g.Name
                        })
                        .ToList();
                    return View(newClass);
                }

                // Set creation timestamp
                newClass.CreateAt = DateTime.Now;

                _db.Classes.Add(newClass);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Class created successfully!";
                return RedirectToAction("IndexClass");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Database Error: {ex.Message}");
            }

            // Repopulate GradeLevel dropdown if an error occurs
            ViewBag.GradeLevels = _db.GradeLevels
                .Select(g => new SelectListItem
                {
                    Value = g.GradeLevelId.ToString(),
                    Text = g.Name
                })
                .ToList();
            return View(newClass);
        }

        [HttpGet]
        [Route("Classes/Edit/{id}")]
        public async Task<IActionResult> EditClass(int id)
        {
            var classToEdit = await _db.Classes
                .Include(c => c.GradeLevels)
                .FirstOrDefaultAsync(c => c.ClassId == id);

            if (classToEdit == null)
            {
                return NotFound();
            }

            // Populate GradeLevel dropdown
            ViewBag.GradeLevels = _db.GradeLevels
                .Where(g => g.Status == "Active")
                .Select(g => new SelectListItem
                {
                    Value = g.GradeLevelId.ToString(),
                    Text = g.Name
                })
                .ToList();

            return View(classToEdit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Classes/Edit/{id}")]
        public async Task<IActionResult> EditClass(Class updatedClass)
        {
            // Find the existing class in the database
            var existingClass = await _db.Classes.FindAsync(updatedClass.ClassId);

            if (existingClass == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                // Repopulate GradeLevel dropdown
                ViewBag.GradeLevels = _db.GradeLevels
                    .Select(g => new SelectListItem
                    {
                        Value = g.GradeLevelId.ToString(),
                        Text = g.Name
                    })
                    .ToList();
                return View(updatedClass);
            }

            try
            {
                // Validate GradeLevel exists
                var gradeLevel = await _db.GradeLevels
                    .FirstOrDefaultAsync(g => g.GradeLevelId == updatedClass.GradeLevelId);

                if (gradeLevel == null)
                {
                    ModelState.AddModelError("GradeLevelId", "Invalid Grade Level selected.");
                    ViewBag.GradeLevels = _db.GradeLevels
                        .Select(g => new SelectListItem
                        {
                            Value = g.GradeLevelId.ToString(),
                            Text = g.Name
                        })
                        .ToList();
                    return View(updatedClass);
                }

                // Check for duplicate class name within the same grade level
                var duplicateClass = await _db.Classes
                    .FirstOrDefaultAsync(c =>
                        c.ClassNumber == updatedClass.ClassNumber &&
                        c.GradeLevelId == updatedClass.GradeLevelId &&
                        c.ClassId != updatedClass.ClassId);

                if (duplicateClass != null)
                {
                    ModelState.AddModelError("ClassName", "A class with this name already exists in the selected grade level.");
                    ViewBag.GradeLevels = _db.GradeLevels
                        .Select(g => new SelectListItem
                        {
                            Value = g.GradeLevelId.ToString(),
                            Text = g.Name
                        })
                        .ToList();
                    return View(updatedClass);
                }

                // Preserve the original CreatedAt timestamp
                updatedClass.CreateAt = existingClass.CreateAt;

                // Update the UpdatedAt timestamp to current time
                updatedClass.UpdateAt = DateTime.Now;

                // Detach the existing entity to avoid tracking conflicts
                _db.Entry(existingClass).State = EntityState.Detached;

                // Update the entire entity
                _db.Classes.Update(updatedClass);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Class updated successfully!";
                return RedirectToAction("IndexClass");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating class: {ex.Message}");

                // Repopulate GradeLevel dropdown
                ViewBag.GradeLevels = _db.GradeLevels
                    .Select(g => new SelectListItem
                    {
                        Value = g.GradeLevelId.ToString(),
                        Text = g.Name
                    })
                    .ToList();
                return View(updatedClass);
            }
        }

        [HttpPost]
        [Route("Classes/Delete/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> DeleteClass(int id)
        {
            try
            {
                var classToDelete = await _db.Classes
                    .FirstOrDefaultAsync(c => c.ClassId == id);

                if (classToDelete == null)
                {
                    TempData["ErrorMessage"] = "Class not found.";
                    return RedirectToAction("Index");
                }

                // Check for related records before deleting
                bool hasRelatedStudents = await _db.Students
                    .AnyAsync(s => s.ClassId == id);

                _db.Classes.Remove(classToDelete);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Class deleted successfully!";
            }
            catch (Exception ex)
            {
                // Consider logging the exception
                TempData["ErrorMessage"] = $"Error deleting class: {ex.Message}";
            }

            return RedirectToAction("IndexClass");
        }

    }
}
