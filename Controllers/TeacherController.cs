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
using Microsoft.AspNetCore.Authorization;

namespace SchoolSystem.Controllers
{
    [Authorize(Policy = "AcademicPolicyOrAdminPolicy")]
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

        [HttpGet]
        [Route("Teacher")]
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
                    (t.Profile.FirstName + " " + t.Profile.LastName).Contains(searchString));
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

        [HttpGet]
        [Route("Teacher/Details")]
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



        [HttpGet]
        [Route("Teacher/Create")]
        public IActionResult CreateTeacher()
        {
            var viewModel = new TeacherViewModel
            {
                Departments = GetDepartmentListItems().Result
            };

            return View(viewModel);
        }
        [HttpPost]
        [Route("Teacher/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTeacher(TeacherViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload the departments dropdown when validation fails
                TempData["ErrorMessage"] = "Please correct the highlighted errors and try again.";
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
                    var errorMessages = result.Errors.Select(e => e.Description).ToList();
                    TempData["ErrorMessage"] = $"User creation failed: {string.Join(", ", errorMessages)}";
                    model.Departments = await GetDepartmentListItems();
                    return View(model);
                }

                // Assign Teacher role
                await _userManager.AddToRoleAsync(user, "Teacher");

                // 2. Create Profile
                var profile = new Profiles
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Gender = model.Gender,
                    Address = model.Address,
                    DateOfBirth = DateOnly.FromDateTime(model.DateOfBirth),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    UserId = user.Id
                };

                // ถ้ามีการอัพโหลดรูปโปรไฟล์
                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    var validationResult = ValidateImageFile(model.ProfilePicture);
                    if (!validationResult.IsValid)
                    {
                        ModelState.AddModelError("ProfilePicture", validationResult.ErrorMessage);
                        model.Departments = await GetDepartmentListItems();
                        return View(model);
                    }

                    // สร้างชื่อไฟล์แบบสุ่มโดยใช้ GUID
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePicture.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles", fileName);

                    // บันทึกไฟล์ลงในโฟลเดอร์ /images/profiles/
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePicture.CopyToAsync(stream);
                    }

                    // บันทึก path ของไฟล์ลงใน ProfilePictureUrl
                    profile.ProfilePictureUrl = $"/images/profiles/{fileName}";
                }
                else
                {
                    profile.ProfilePictureUrl = ""; // หรือกำหนดค่า default ถ้าต้องการ
                }

                _db.Profiles.Add(profile);
                await _db.SaveChangesAsync();

                // 3. Create Teacher record
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

                await _db.Teachers.AddAsync(teacher);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "Teacher created successfully!";
                return RedirectToAction("IndexTeacher");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating teacher: {ex.Message}");
                _logger.LogError(ex, "Error creating teacher: {ErrorMessage}", ex.Message);
                TempData["ErrorMessage"] = $"Teacher creation failed: {ex.Message}. Please try again or contact support.";
                model.Departments = await GetDepartmentListItems();
                return View(model);
            }
        }


        // Helper method to get department list items
        private async Task<List<SelectListItem>> GetDepartmentListItems()
        {
            return await _db.Departments
                .Where(d => d.Status == "Active")
                .Select(d => new SelectListItem
                {
                    Value = d.DepartmentId.ToString(),
                    Text = d.Name
                })
                .ToListAsync();
        }

        [HttpGet]
        [Route("Teacher/Edit/{id}")]
        public async Task<IActionResult> EditTeacher(int id)
        {
            try
            {
                var teacher = await _db.Teachers
                    .Include(t => t.Profile)
                        .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(t => t.TeacherId == id);

                if (teacher == null)
                {
                    return NotFound($"Teacher with ID {id} not found.");
                }

                var model = new TeacherViewEditModel
                {
                    TeacherId = teacher.TeacherId,
                    ProfileId = teacher.ProfileId ?? 0,
                    FirstName = teacher.Profile?.FirstName ?? string.Empty,
                    LastName = teacher.Profile?.LastName ?? string.Empty,
                    Gender = teacher.Profile?.Gender ?? "Not Specified",
                    Address = teacher.Profile?.Address ?? string.Empty,
                    DateOfBirth = teacher.Profile?.DateOfBirth.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
                    ProfilePictureUrl = teacher.Profile?.ProfilePictureUrl ?? string.Empty,
                    DepartmentId = teacher.DepartmentId,
                    HireDate = teacher.HireDate,
                    Salary = teacher.Salary,
                    Status = teacher.Status ?? string.Empty,
                    Email = teacher.Profile?.User?.Email ?? string.Empty,
                    Username = teacher.Profile?.User?.UserName ?? string.Empty,
                    Role = "Teacher"
                };

                model.Departments = await GetDepartmentListItems();
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading teacher with id {TeacherId}", id);
                return StatusCode(500, "An error occurred while loading the teacher's information.");
            }
        }

        [HttpPost]
        [Route("Teacher/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTeacher(TeacherViewEditModel model, int id)
        {
            if (!ModelState.IsValid)
            {
                model.Departments = await GetDepartmentListItems();
                TempData["ErrorMessage"] = "Please correct the highlighted errors and try again.";
                return View(model);
            }

            var teacher = await _db.Teachers
                .Include(t => t.Profile)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(t => t.TeacherId == id);

            if (teacher == null)
            {
                TempData["ErrorMessage"] = $"Teacher with ID {id} not found. Unable to update.";
                return NotFound($"Teacher with ID {model.TeacherId} not found.");
            }

            try
            {
                // Update Profile
                teacher.Profile.FirstName = model.FirstName;
                teacher.Profile.LastName = model.LastName;
                teacher.Profile.Gender = model.Gender;
                teacher.Profile.Address = model.Address;
                teacher.Profile.DateOfBirth = DateOnly.FromDateTime(model.DateOfBirth);

                // Handle Profile Picture
                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    var validationResult = ValidateImageFile(model.ProfilePicture);
                    if (!validationResult.IsValid)
                    {
                        ModelState.AddModelError("ProfilePicture", validationResult.ErrorMessage);
                        model.Departments = await GetDepartmentListItems();
                        return View(model);
                    }

                    // Delete old profile picture
                    if (!string.IsNullOrEmpty(teacher.Profile.ProfilePictureUrl))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", teacher.Profile.ProfilePictureUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Save new profile picture
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ProfilePicture.FileName)}";
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePicture.CopyToAsync(stream);
                    }

                    teacher.Profile.ProfilePictureUrl = $"/images/profiles/{fileName}";
                }

                // Update User Account
                if (teacher.Profile.User != null)
                {
                    teacher.Profile.User.Email = model.Email;
                    teacher.Profile.User.UserName = model.Username;

                    if (!string.IsNullOrWhiteSpace(model.Password))
                    {
                        var passwordHasher = new PasswordHasher<Users>();
                        teacher.Profile.User.PasswordHash = passwordHasher.HashPassword(teacher.Profile.User, model.Password);
                    }
                }

                // Update Teacher Information
                teacher.DepartmentId = (model.DepartmentId.HasValue && model.DepartmentId.Value > 0) ? model.DepartmentId : null;
                teacher.HireDate = model.HireDate;
                teacher.Salary = model.Salary;
                teacher.Status = model.Status;
                teacher.UpdateAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Teacher {model.FirstName} {model.LastName} has been updated successfully!";
                return RedirectToAction(nameof(IndexTeacher));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Update failed: {ex.Message}. Please try again or contact support.";
                _logger.LogError(ex, "Error updating teacher with id {TeacherId}", model.TeacherId);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the teacher.");
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

                // ตรวจสอบและลบไฟล์ภาพโปรไฟล์ (ถ้ามี)
                if (!string.IsNullOrEmpty(teacher.Profile?.ProfilePictureUrl))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", teacher.Profile.ProfilePictureUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // ลบ Teacher และ Profile ที่เกี่ยวข้อง
                _db.Teachers.Remove(teacher);
                _db.Profiles.Remove(teacher.Profile);
                await _db.SaveChangesAsync();

                // ลบ User ที่เชื่อมโยงกับ Profile (รวมถึง Role ใน aspnetUserRoles ด้วย)
                if (!string.IsNullOrEmpty(teacher.Profile.UserId))
                {
                    var user = await _userManager.FindByIdAsync(teacher.Profile.UserId);
                    if (user != null)
                    {
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

        private (bool IsValid, string ErrorMessage) ValidateImageFile(IFormFile file)
        {
            // ตรวจสอบขนาดไฟล์ (จำกัดที่ 5MB)
            if (file.Length > 5 * 1024 * 1024)
            {
                return (false, "ขนาดไฟล์เกิน 5MB");
            }

            // ตรวจสอบนามสกุลไฟล์
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return (false, "ประเภทไฟล์ไม่ถูกต้อง ต้องเป็น .jpg, .jpeg, .png, .gif หรือ .bmp เท่านั้น");
            }

            // ตรวจสอบ MIME Type
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return (false, "ประเภทไฟล์ไม่ถูกต้อง ต้องเป็นภาพเท่านั้น");
            }

            // ตรวจสอบเนื้อหาของไฟล์ว่าเป็นภาพจริงหรือไม่
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var image = SixLabors.ImageSharp.Image.Load(stream);
                    // ถ้าโหลดสำเร็จ แปลว่าเป็นภาพที่ถูกต้อง
                }
            }
            catch
            {
                return (false, "ไฟล์นี้ไม่ใช่ภาพที่ถูกต้อง");
            }

            return (true, null);
        }




    }
}
