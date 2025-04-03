using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Helpers;
using SchoolSystem.Models.UserManagement;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSystem.Controllers
{
    [Authorize(Policy = "AcademicPolicyOrAdminPolicy")]
    public class DepartmentController : Controller
    {
        private readonly AppDbContext _db;

        public DepartmentController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Route("Departments")]
        public async Task<IActionResult> Index(int? pageNumber, string searchString, string statusFilter)
        {
            // เก็บข้อมูลการกรองที่ใช้ค้นหาไว้ใน ViewData
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = statusFilter;

            // Populate Status filter dropdown (เช่น Active, Inactive หรืออื่น ๆ ที่มีอยู่)
            var statuses = await _db.Departments
                .Where(d => d.Status != null)
                .Select(d => d.Status)
                .Distinct()
                .ToListAsync();
            ViewData["Statuses"] = statuses
                .Select(s => new SelectListItem { Value = s, Text = s })
                .ToList();

            // สร้าง base query สำหรับ Department
            var departmentsQuery = _db.Departments
                .AsNoTracking();

            // กรองจากคำค้นหา (ค้นหาจาก Name หรือ Description)
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                departmentsQuery = departmentsQuery.Where(d =>
                    d.Name.Contains(searchString) || d.Description.Contains(searchString));
            }

            // กรองตาม Status ที่เลือก (ถ้ามี)
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                departmentsQuery = departmentsQuery.Where(d => d.Status == statusFilter);
            }

            // เรียงลำดับข้อมูล โดยเรียงตาม Name ของ Department
            departmentsQuery = departmentsQuery.OrderBy(d => d.Name);

            // การแบ่งหน้า
            int pageSize = 10;
            var totalItems = await departmentsQuery.CountAsync();
            ViewData["TotalItems"] = totalItems;
            var pagedDepartments = await PaginatedList<Department>.CreateAsync(departmentsQuery, pageNumber ?? 1, pageSize);

            return View(pagedDepartments);
        }


        [HttpGet]
        [Route("Departments/Details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var departmentDetails = await _db.Departments
                .FirstOrDefaultAsync(d => d.DepartmentId == id);

            if (departmentDetails == null)
                return NotFound();

            return View(departmentDetails);
        }


        [HttpGet]
        [Route("Departments/Create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Route("Departments/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Department newDepartment)
        {
            if (!ModelState.IsValid)
            {
                return View(newDepartment);
            }

            try
            {
                // ตรวจสอบว่ามี Department ที่มีชื่อเดียวกันอยู่แล้วหรือไม่ (ไม่ให้ซ้ำ)
                var existingDepartment = await _db.Departments
                    .FirstOrDefaultAsync(d => d.Name.ToLower() == newDepartment.Name.ToLower());

                if (existingDepartment != null)
                {
                    ModelState.AddModelError("Name", "A department with this name already exists.");
                    TempData["ErrorMessage"] = $"A department with this name{newDepartment.Name} already exists.";
                    return View(newDepartment);
                }

                newDepartment.CreatedAt = DateTime.UtcNow;
                newDepartment.UpdatedAt = DateTime.UtcNow;

                _db.Departments.Add(newDepartment);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Department created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Database Error: {ex.Message}");
            }

            // ในกรณีเกิด error ให้ repopulate dropdown สำหรับ Status
            ViewBag.Statuses = new List<SelectListItem>
            {
                new SelectListItem { Value = "Active", Text = "Active" },
                new SelectListItem { Value = "Inactive", Text = "Inactive" }
            };
            return View(newDepartment);
        }


        [HttpGet]
        [Route("Departments/Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var departmentToEdit = await _db.Departments.FindAsync(id);
            if (departmentToEdit == null)
            {
                return NotFound();
            }

            // Populate Status dropdown
            ViewBag.Statuses = new List<SelectListItem>
    {
        new SelectListItem { Value = "Active", Text = "Active" },
        new SelectListItem { Value = "Inactive", Text = "Inactive" }
    };

            return View(departmentToEdit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Departments/Edit/{id}")]
        public async Task<IActionResult> Edit(Department updatedDepartment)
        {
            // Find the existing department in the database
            var existingDepartment = await _db.Departments.FindAsync(updatedDepartment.DepartmentId);
            if (existingDepartment == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                // Repopulate Status dropdown
                ViewBag.Statuses = new List<SelectListItem>
        {
            new SelectListItem { Value = "Active", Text = "Active" },
            new SelectListItem { Value = "Inactive", Text = "Inactive" }
        };
                return View(updatedDepartment);
            }

            try
            {
                // Check for duplicate department name (ไม่ให้มีชื่อซ้ำกัน ยกเว้นตัวเอง)
                var duplicateDepartment = await _db.Departments
                    .FirstOrDefaultAsync(d => d.Name.ToLower() == updatedDepartment.Name.ToLower() &&
                                                d.DepartmentId != updatedDepartment.DepartmentId);
                if (duplicateDepartment != null)
                {
                    ModelState.AddModelError("Name", "A department with this name already exists.");
                    ViewBag.Statuses = new List<SelectListItem>
            {
                new SelectListItem { Value = "Active", Text = "Active" },
                new SelectListItem { Value = "Inactive", Text = "Inactive" }
            };
                    return View(updatedDepartment);
                }

                // Preserve the original CreatedAt timestamp
                updatedDepartment.CreatedAt = existingDepartment.CreatedAt;
                // Update the UpdatedAt timestamp to current time
                updatedDepartment.UpdatedAt = DateTime.UtcNow;

                // Detach the existing entity to avoid tracking conflicts
                _db.Entry(existingDepartment).State = EntityState.Detached;

                // Update the entity
                _db.Departments.Update(updatedDepartment);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Department updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating department: {ex.Message}");
                // Repopulate Status dropdown
                ViewBag.Statuses = new List<SelectListItem>
        {
            new SelectListItem { Value = "Active", Text = "Active" },
            new SelectListItem { Value = "Inactive", Text = "Inactive" }
        };
                return View(updatedDepartment);
            }
        }


        [HttpPost]
        [Route("Departments/Delete/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var departmentToDelete = await _db.Departments.FirstOrDefaultAsync(d => d.DepartmentId == id);
                if (departmentToDelete == null)
                {
                    TempData["ErrorMessage"] = "Department not found.";
                    return RedirectToAction("IndexDepartment");
                }

                // Optionally, check for related Teachers before deletion.
                bool hasRelatedTeachers = await _db.Teachers.AnyAsync(t => t.DepartmentId == id);
                if (hasRelatedTeachers)
                {
                    TempData["ErrorMessage"] = "Cannot delete department with related teachers.";
                    return RedirectToAction("IndexDepartment");
                }

                _db.Departments.Remove(departmentToDelete);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Department deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting department: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
