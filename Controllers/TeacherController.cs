using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models.UserManagement;
using SchoolSystem.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using SchoolSystem.Helpers;
using Microsoft.AspNetCore.Identity;

namespace SchoolSystem.Controllers
{
    public class TeacherController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<TeacherController> _logger;

        public TeacherController(UserManager<Users> userManager, RoleManager<IdentityRole> roleManager, AppDbContext db, ILogger<TeacherController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
            _logger = logger;
        }
        // GET: Teacher
        public async Task<IActionResult> IndexTeacher(int? pageNumber, string searchString, string sortOrder, int? departmentFilter)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["SalarySortParam"] = sortOrder == "salary" ? "salary_desc" : "salary";
            ViewData["DepartmentSortParam"] = sortOrder == "department" ? "department_desc" : "department";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentDepartment"] = departmentFilter;

            // ดึงข้อมูล department สำหรับ dropdown (อาจมีการนำไปใช้ใน View เพิ่มเติม)
            var departments = await GetDepartmentListItems();
            ViewData["Departments"] = departments;

            int pageSize = 10;
            var teachersQuery = _db.Teachers
                .Include(t => t.Profile)
                .AsNoTracking();

            // ค้นหาตามชื่อ (first name หรือ last name)
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                teachersQuery = teachersQuery.Where(t =>
                    t.Profile.FirstName.Contains(searchString) ||
                    t.Profile.LastName.Contains(searchString));
            }

            // กรองตามแผนก
            if (departmentFilter.HasValue)
            {
                teachersQuery = teachersQuery.Where(t => t.DepartmentId == departmentFilter);
            }

            int totalItems = await teachersQuery.CountAsync();
            ViewData["TotalItems"] = totalItems;

            // การจัดเรียงข้อมูล
            teachersQuery = sortOrder switch
            {
                "name_desc" => teachersQuery.OrderByDescending(t => t.Profile.LastName).ThenByDescending(t => t.Profile.FirstName),
                "salary" => teachersQuery.OrderBy(t => t.Salary),
                "salary_desc" => teachersQuery.OrderByDescending(t => t.Salary),
                "department" => teachersQuery.OrderBy(t => t.DepartmentId),
                "department_desc" => teachersQuery.OrderByDescending(t => t.DepartmentId),
                _ => teachersQuery.OrderBy(t => t.Profile.LastName).ThenBy(t => t.Profile.FirstName),
            };

            return View(await PaginatedList<Teacher>.CreateAsync(teachersQuery, pageNumber ?? 1, pageSize));
        }


        public async Task<IActionResult> DetailsTeacher(int id)
        {
            try
            {
                // Fetch Teacher with related Profile and Department info
                var teacher = await _db.Teachers
                    .Include(t => t.Profile)
                    .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(t => t.TeacherId == id);

                if (teacher == null)
                {
                    TempData["ErrorMessage"] = "Teacher not found.";
                    return RedirectToAction("IndexTeacher");
                }
                if (teacher.Profile == null || teacher.Profile.User == null)
                {
                    TempData["ErrorMessage"] = "Teacher profile or user information is incomplete.";
                    return RedirectToAction("IndexTeacher");
                }
                // Prepare view model with teacher data
                var model = new TeacherViewEditModel
                {
                    TeacherId = id,
                    Email = teacher.Profile.User.Email,
                    Username = teacher.Profile.User.UserName,
                    FirstName = teacher.Profile.FirstName,
                    LastName = teacher.Profile.LastName,
                    Gender = teacher.Profile.Gender,
                    Address = teacher.Profile.Address,
                    DateOfBirth = teacher.Profile.DateOfBirth.ToDateTime(new TimeOnly(0, 0)),
                    ProfilePictureUrl = teacher.Profile.ProfilePictureUrl,
                    DepartmentId = teacher.DepartmentId,
                    HireDate = teacher.HireDate,
                    Salary = teacher.Salary,
                    Status = teacher.Status,
                    Departments = await GetDepartmentListItems()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading teacher details: {ex.Message}";
                return RedirectToAction("IndexTeacher");
            }
        }




        public IActionResult CreateTeacher()
        {
            var viewModel = new TeacherViewModel
            {
                Departments = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "Select Department (Optional)" },
                    new SelectListItem { Value = "1", Text = "Mathematics" },
                    new SelectListItem { Value = "2", Text = "Science" },
                    new SelectListItem { Value = "3", Text = "English" },
                    new SelectListItem { Value = "4", Text = "History" },
                    new SelectListItem { Value = "5", Text = "Computer Science" }
                }
            };

            return View(viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTeacher(TeacherViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload the departments dropdown when validation fails
                model.Departments = await GetDepartmentListItems();
                return View(model);
            }

            try
            {
                using var transaction = await _db.Database.BeginTransactionAsync();

                // 1. Create User account
                var user = new Users
                {
                    UserName = model.Username,
                    Email = model.Email,
                    Status = "Active",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var passwordHasher = new PasswordHasher<Users>();
                user.PasswordHash = passwordHasher.HashPassword(user, model.Password);
                var result = await _userManager.CreateAsync(user);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    model.Departments = await GetDepartmentListItems();
                    return View(model);
                }

                // Assign Teacher role
                await _userManager.AddToRoleAsync(user, "Teacher");

                // 2. Create Profile
                // สร้าง Profile
                var profile = new Profiles
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Gender = model.Gender,
                    Address = model.Address,
                    DateOfBirth = DateOnly.FromDateTime(model.DateOfBirth),
                    ProfilePictureUrl = string.IsNullOrEmpty(model.ProfilePictureUrl) ? "" : model.ProfilePictureUrl,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    UserId = user.Id
                };

                // บันทึก Profile และรับค่า ProfileId
                _db.Profiles.Add(profile);
                await _db.SaveChangesAsync();

                var teacher = new Teacher
                {
                    ProfileId = profile.ProfileId, // ใช้ ProfileId ที่ได้จากการบันทึก Profile
                    DepartmentId = (model.DepartmentId.HasValue && model.DepartmentId.Value > 0) ? model.DepartmentId.Value : null,
                    HireDate = model.HireDate,
                    Salary = model.Salary,
                    Status = model.Status,
                    CreateAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow
                };

                // Add teacher record and save immediately
                await _db.Teachers.AddAsync(teacher);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Teacher created successfully!";
                return RedirectToAction("IndexTeacher");
            }
            catch (Exception ex)
            {
                // Roll back transaction in case of error
                ModelState.AddModelError("", $"Error creating teacher: {ex.Message}");
                _logger.LogError(ex, "Error creating teacher: {ErrorMessage}", ex.Message);

                // Ensure dropdown data is reloaded
                model.Departments = await GetDepartmentListItems();
                return View(model);
            }
        }

        // Helper method to get department list items
        private async Task<List<SelectListItem>> GetDepartmentListItems()
        {
            // Ideally, fetch from database
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "Select Department (Optional)" },
                new SelectListItem { Value = "1", Text = "Mathematics" },
                new SelectListItem { Value = "2", Text = "Science" },
                new SelectListItem { Value = "3", Text = "English" },
                new SelectListItem { Value = "4", Text = "History" },
                new SelectListItem { Value = "5", Text = "Computer Science" }
            };
        }

        [HttpGet]
        public async Task<IActionResult> EditTeacher(int id)
        {
            try
            {
                // ดึงข้อมูล teacher พร้อมทั้ง Profile และ User (ถ้ามี)
                var teacher = await _db.Teachers
                    .Include(t => t.Profile)
                        .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(t => t.TeacherId == id);

                if (teacher == null)
                {
                    TempData["ErrorMessage"] = "Teacher not found.";
                    return RedirectToAction(nameof(IndexTeacher));
                }

                var model = new SchoolSystem.ViewModels.TeacherViewEditModel
                {
                    TeacherId = teacher.TeacherId,
                    ProfileId = teacher.ProfileId ?? 0, // ตรวจสอบ null หากมีความเป็นไปได้
                    FirstName = teacher.Profile?.FirstName,
                    LastName = teacher.Profile?.LastName,
                    Gender = teacher.Profile?.Gender,
                    Address = teacher.Profile?.Address,
                    DateOfBirth = teacher.Profile != null
                        ? teacher.Profile.DateOfBirth.ToDateTime(TimeOnly.MinValue)
                        : DateTime.MinValue,
                    ProfilePictureUrl = teacher.Profile?.ProfilePictureUrl,
                    DepartmentId = teacher.DepartmentId,
                    HireDate = teacher.HireDate,
                    Salary = teacher.Salary,
                    Status = teacher.Status,
                    Email = teacher.Profile?.User != null ? teacher.Profile.User.Email : string.Empty,
                    Username = teacher.Profile?.User != null ? teacher.Profile.User.UserName : string.Empty,
                    Role = "Teacher"
                };

                model.Departments = await GetDepartmentListItems();

                return View(model);
            }
            catch (Exception ex)
            {
                // บันทึก log เพื่อช่วยแก้ไขปัญหา
                _logger.LogError(ex, "Error loading teacher with id {TeacherId}", id);
                TempData["ErrorMessage"] = $"Error loading teacher: {ex.Message}";
                return RedirectToAction(nameof(IndexTeacher));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTeacher(SchoolSystem.ViewModels.TeacherViewEditModel model)
        {
            if (!ModelState.IsValid)
            {
                // เมื่อเกิด validation error ให้ repopulate dropdown list
                model.Departments = await GetDepartmentListItems();
                return View(model);
            }

            // ดึงข้อมูล teacher พร้อม Profile และ User ที่เกี่ยวข้อง
            var teacher = await _db.Teachers
                .Include(t => t.Profile)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(t => t.TeacherId == model.TeacherId);

            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Teacher not found.";
                return RedirectToAction(nameof(IndexTeacher));
            }

            try
            {
                // ปรับปรุงข้อมูล Profile
                teacher.Profile.FirstName = model.FirstName;
                teacher.Profile.LastName = model.LastName;
                teacher.Profile.Gender = model.Gender;
                teacher.Profile.Address = model.Address;
                teacher.Profile.DateOfBirth = DateOnly.FromDateTime(model.DateOfBirth);
                teacher.Profile.ProfilePictureUrl = model.ProfilePictureUrl;

                // ปรับปรุง Account Info หากมีการเปลี่ยนแปลง
                if (teacher.Profile.User != null)
                {
                    teacher.Profile.User.Email = model.Email;
                    teacher.Profile.User.UserName = model.Username;

                    // ถ้ามีการกรอกรหัสผ่านใหม่ (ไม่ใช่ค่าว่าง) ให้ update รหัสผ่าน
                    if (!string.IsNullOrWhiteSpace(model.Password))
                    {
                        // ควรใช้ UserManager ในการจัดการการเปลี่ยนแปลงรหัสผ่านอย่างปลอดภัย
                        var passwordHasher = new PasswordHasher<Users>();
                        teacher.Profile.User.PasswordHash = passwordHasher.HashPassword(teacher.Profile.User, model.Password);
                    }
                }

                // ปรับปรุงข้อมูล Teacher
                teacher.DepartmentId = (model.DepartmentId.HasValue && model.DepartmentId.Value > 0) ? model.DepartmentId : null;
                teacher.HireDate = model.HireDate;
                teacher.Salary = model.Salary;
                teacher.Status = model.Status;
                teacher.UpdateAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Teacher updated successfully!";
                return RedirectToAction(nameof(IndexTeacher));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating teacher with id {TeacherId}", model.TeacherId);
                TempData["ErrorMessage"] = $"Error updating teacher: {ex.Message}";
                model.Departments = await GetDepartmentListItems();
                return View(model);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            try
            {
                // ดึงข้อมูล Teacher พร้อมกับ Profile ที่เกี่ยวข้อง
                var teacher = await _db.Teachers
                    .Include(t => t.Profile)
                    .FirstOrDefaultAsync(t => t.TeacherId == id);

                if (teacher == null)
                {
                    TempData["ErrorMessage"] = "Teacher not found.";
                    return RedirectToAction("IndexTeacher");
                }

                // ลบ Teacher และ Profile ที่เกี่ยวข้องก่อน
                _db.Teachers.Remove(teacher);
                _db.Profiles.Remove(teacher.Profile);
                await _db.SaveChangesAsync();

                // ลบ User ที่เชื่อมโยงกับ Profile (รวมถึง Role ใน aspnetUserRoles ด้วย)
                if (!string.IsNullOrEmpty(teacher.Profile.UserId))
                {
                    var user = await _userManager.FindByIdAsync(teacher.Profile.UserId);
                    if (user != null)
                    {
                        // การลบ User ผ่าน _userManager จะทำการลบ Role ใน aspnetUserRoles อัตโนมัติ
                        var result = await _userManager.DeleteAsync(user);
                        if (!result.Succeeded)
                        {
                            TempData["ErrorMessage"] = "User deletion failed.";
                            return RedirectToAction("IndexTeacher");
                        }
                    }
                }

                TempData["SuccessMessage"] = "Teacher deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting teacher: {ex.Message}";
            }

            return RedirectToAction("IndexTeacher");
        }




    }
}
