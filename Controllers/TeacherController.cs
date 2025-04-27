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

            var departments = await GetDepartmentListItems();
            ViewData["Departments"] = departments;

            int pageSize = 10;
            var teachersQuery = _db.Teachers
                .Include(t => t.Profile)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                teachersQuery = teachersQuery.Where(t =>
                    (t.Profile.FirstName + " " + t.Profile.LastName).Contains(searchString));
            }

            if (departmentFilter.HasValue)
            {
                teachersQuery = teachersQuery.Where(t => t.DepartmentId == departmentFilter);
            }

            int totalItems = await teachersQuery.CountAsync();
            ViewData["TotalItems"] = totalItems;

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

        /*
         แบ่งหน้าที่ยังไง
         */

        [HttpGet]
        [Route("Teacher/Details")]
        public async Task<IActionResult> DetailsTeacher(int id)
        {
            try
            {
                var teacher = await _db.Teachers
                    .Include(t => t.Profile)
                    .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(t => t.TeacherId == id);

                if (teacher == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบครูที่ค้นหา";
                    return RedirectToAction("IndexTeacher");
                }
                if (teacher.Profile == null || teacher.Profile.User == null)
                {
                    TempData["ErrorMessage"] = "ข้อมูลโปรไฟล์หรือผู้ใช้ของครูไม่สมบูรณ์";
                    return RedirectToAction("IndexTeacher");
                }
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
                    HasAcademicRole = await _db.UserRoles
                        .AnyAsync(ur => ur.UserId == teacher.Profile.UserId && _roleManager.Roles.Any(r => r.Id == ur.RoleId && r.Name == "Academic")),
                    Departments = await GetDepartmentListItems()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"เกิดข้อผิดพลาดในการโหลดข้อมูลครู: {ex.Message}";
                return RedirectToAction("IndexTeacher");
            }
        }

        [HttpPost]
        public async Task<IActionResult> AssignAcademicRank(int id)
        {
            var teacher = await _db.Teachers
                .Include(t => t.Profile)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(t => t.TeacherId == id);

            if (teacher == null || teacher.Profile == null || teacher.Profile.User == null)
            {
                return NotFound("ไม่พบครูหรือผู้ใช้ที่เกี่ยวข้อง");
            }

            var user = teacher.Profile.User;
            var roleName = "Academic";

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                return BadRequest("ไม่พบบทบาทที่ระบุ");
            }

            var addResult = await _userManager.AddToRoleAsync(user, roleName);

            if (!addResult.Succeeded)
            {
                return BadRequest("ไม่สามารถมอบหมายบทบาทได้");
            }
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "มอบหมายบทบาทสำเร็จ";
            return RedirectToAction("DetailsTeacher", new { id });
        }

        [HttpPost]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> RemoveAcademicRank(int id)
        {
            var teacher = await _db.Teachers
                .Include(t => t.Profile)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(t => t.TeacherId == id);

            if (teacher == null || teacher.Profile == null || teacher.Profile.User == null)
            {
                return NotFound("ไม่พบครูหรือผู้ใช้ที่เกี่ยวข้อง");
            }

            var user = teacher.Profile.User;
            var roleName = "Academic";

            var removeResult = await _userManager.RemoveFromRoleAsync(user, roleName);

            if (!removeResult.Succeeded)
            {
                return BadRequest("ไม่สามารถยกเลิกบทบาทได้");
            }

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "ยกเลิกบทบาทสำเร็จ";
            return RedirectToAction("DetailsTeacher", new { id });
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
                TempData["ErrorMessage"] = "กรุณาแก้ไขข้อผิดพลาดที่แสดงแล้วลองใหม่อีกครั้ง";
                model.Departments = await GetDepartmentListItems();
                return View(model);
            }

            try
            {
                using var transaction = await _db.Database.BeginTransactionAsync();

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
                    var errorMessages = result.Errors.Select(e => e.Description).ToList();
                    TempData["ErrorMessage"] = $"การสร้างผู้ใช้ล้มเหลว: {string.Join(", ", errorMessages)}";
                    model.Departments = await GetDepartmentListItems();
                    return View(model);
                }

                await _userManager.AddToRoleAsync(user, "Teacher");

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

                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    var validationResult = ValidateImageFile(model.ProfilePicture);
                    if (!validationResult.IsValid)
                    {
                        ModelState.AddModelError("ProfilePicture", validationResult.ErrorMessage);
                        model.Departments = await GetDepartmentListItems();
                        return View(model);
                    }

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePicture.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePicture.CopyToAsync(stream);
                    }

                    profile.ProfilePictureUrl = $"/images/profiles/{fileName}";
                }
                else
                {
                    profile.ProfilePictureUrl = "";
                }

                _db.Profiles.Add(profile);
                await _db.SaveChangesAsync();

                var teacher = new Teacher
                {
                    ProfileId = profile.ProfileId,
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

                TempData["SuccessMessage"] = "สร้างครูเรียบร้อยแล้ว";
                return RedirectToAction("IndexTeacher");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"เกิดข้อผิดพลาดในการสร้างครู: {ex.Message}");
                _logger.LogError(ex, "Error creating teacher: {ErrorMessage}", ex.Message);
                TempData["ErrorMessage"] = $"การสร้างครูล้มเหลว: {ex.Message}. กรุณาลองใหม่หรือแจ้งเจ้าหน้าที่";
                model.Departments = await GetDepartmentListItems();
                return View(model);
            }
        }

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
                    return NotFound($"ไม่พบครูที่มี ID {id}");
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
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการโหลดข้อมูลครูที่มี id {TeacherId}", id);
                return StatusCode(500, "เกิดข้อผิดพลาดในการโหลดข้อมูลของครู");
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
                TempData["ErrorMessage"] = "กรุณาแก้ไขข้อผิดพลาดที่แสดงแล้วลองใหม่อีกครั้ง";
                return View(model);
            }

            var teacher = await _db.Teachers
                .Include(t => t.Profile)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(t => t.TeacherId == id);

            if (teacher == null)
            {
                TempData["ErrorMessage"] = $"ไม่พบครูที่มี ID {id} ไม่สามารถอัปเดตข้อมูลได้";
                return NotFound($"ไม่พบครูที่มี ID {model.TeacherId}");
            }

            try
            {
                teacher.Profile.FirstName = model.FirstName;
                teacher.Profile.LastName = model.LastName;
                teacher.Profile.Gender = model.Gender;
                teacher.Profile.Address = model.Address;
                teacher.Profile.DateOfBirth = DateOnly.FromDateTime(model.DateOfBirth);

                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    var validationResult = ValidateImageFile(model.ProfilePicture);
                    if (!validationResult.IsValid)
                    {
                        ModelState.AddModelError("ProfilePicture", validationResult.ErrorMessage);
                        model.Departments = await GetDepartmentListItems();
                        return View(model);
                    }

                    if (!string.IsNullOrEmpty(teacher.Profile.ProfilePictureUrl))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", teacher.Profile.ProfilePictureUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ProfilePicture.FileName)}";
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePicture.CopyToAsync(stream);
                    }

                    teacher.Profile.ProfilePictureUrl = $"/images/profiles/{fileName}";
                }

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

                teacher.DepartmentId = (model.DepartmentId.HasValue && model.DepartmentId.Value > 0) ? model.DepartmentId : null;
                teacher.HireDate = model.HireDate;
                teacher.Salary = model.Salary;
                teacher.Status = model.Status;
                teacher.UpdateAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"ครู {model.FirstName} {model.LastName} ถูกอัปเดตเรียบร้อยแล้ว";
                return RedirectToAction(nameof(IndexTeacher));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"การอัปเดตข้อมูลล้มเหลว: {ex.Message}. กรุณาลองใหม่อีกครั้งหรือแจ้งเจ้าหน้าที่";
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการอัปเดตข้อมูลครูที่มี id {TeacherId}", model.TeacherId);
                ModelState.AddModelError(string.Empty, "เกิดข้อผิดพลาดที่ไม่คาดคิดในการอัปเดตข้อมูลครู");
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
                var teacher = await _db.Teachers
                    .Include(t => t.Profile)
                    .FirstOrDefaultAsync(t => t.TeacherId == id);

                if (teacher == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบครูที่ค้นหา";
                    return RedirectToAction("IndexTeacher");
                }

                if (!string.IsNullOrEmpty(teacher.Profile?.ProfilePictureUrl))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", teacher.Profile.ProfilePictureUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _db.Teachers.Remove(teacher);
                _db.Profiles.Remove(teacher.Profile);
                await _db.SaveChangesAsync();

                if (!string.IsNullOrEmpty(teacher.Profile.UserId))
                {
                    var user = await _userManager.FindByIdAsync(teacher.Profile.UserId);
                    if (user != null)
                    {
                        var result = await _userManager.DeleteAsync(user);
                        if (!result.Succeeded)
                        {
                            TempData["ErrorMessage"] = "การลบผู้ใช้ล้มเหลว";
                            return RedirectToAction("IndexTeacher");
                        }
                    }
                }

                TempData["SuccessMessage"] = "ลบครูเรียบร้อยแล้ว";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"เกิดข้อผิดพลาดในการลบครู: {ex.Message}";
            }

            return RedirectToAction("IndexTeacher");
        }

        private (bool IsValid, string ErrorMessage) ValidateImageFile(IFormFile file)
        {
            if (file.Length > 5 * 1024 * 1024)
            {
                return (false, "ขนาดไฟล์เกิน 5MB");
            }

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return (false, "ประเภทไฟล์ไม่ถูกต้อง ต้องเป็น .jpg, .jpeg, .png, .gif หรือ .bmp เท่านั้น");
            }

            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                return (false, "ประเภทไฟล์ไม่ถูกต้อง ต้องเป็นภาพเท่านั้น");
            }

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var image = SixLabors.ImageSharp.Image.Load(stream);
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
