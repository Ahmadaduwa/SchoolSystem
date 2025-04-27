using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models.UserManagement;
using SchoolSystem.ViewModels;
using SchoolSystem.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUglify.Helpers;
using Microsoft.AspNetCore.Authorization;
using NuGet.DependencyResolver;

namespace SchoolSystem.Controllers
{
    [Authorize(Policy = "AcademicPolicyOrAdminPolicy")]
    public class StudentController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<Users> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<TeacherController> _logger;

        public StudentController(UserManager<Users> userManager, RoleManager<IdentityRole> roleManager, AppDbContext db, ILogger<TeacherController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        [Route("Student")]
        public async Task<IActionResult> IndexStudent(int? pageNumber, string searchString, string sortOrder, int? classFilter)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["CodeSortParam"] = sortOrder == "code" ? "code_desc" : "code";
            ViewData["GpaSortParam"] = sortOrder == "gpa" ? "gpa_desc" : "gpa";
            ViewData["DateSortParam"] = sortOrder == "date" ? "date_desc" : "date";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentClass"] = classFilter; // เก็บค่าสำหรับการกรองห้อง

            // ดึงข้อมูลห้องโดย join ตาราง Class กับ GradeLevels เพื่อใช้ใน Dropdown
            var classList = await _db.Classes
                .Where(g => g.Status == "Active")
                .Include(c => c.GradeLevels)
                .Select(c => new SelectListItem
                {
                    Value = c.ClassId.ToString(),
                    Text = $"{c.GradeLevels.Name}/{c.ClassNumber}" // ตัวอย่าง "ป.1/1"
                })
                .ToListAsync();
            // เพิ่มตัวเลือก "All Classes" ไว้ที่หัวรายการ
            classList.Insert(0, new SelectListItem { Value = "", Text = "ระดับชั้นเรียนทั้งหมด" });
            ViewData["Classes"] = classList;

            int pageSize = 10;
            var studentsQuery = _db.Students
                .Include(s => s.Profile)
                .AsNoTracking();

            // ค้นหาตามชื่อ (FirstName หรือ LastName) หรือ StudentCode
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                studentsQuery = studentsQuery.Where(s =>
                    (s.Profile.FirstName + " " + s.Profile.LastName).Contains(searchString) ||
                    s.Student_Code.Contains(searchString));
            }

            // กรองตามห้อง (Class)
            if (classFilter.HasValue)
            {
                studentsQuery = studentsQuery.Where(s => s.ClassId == classFilter);
            }

            // การจัดเรียงข้อมูล
            studentsQuery = sortOrder switch
            {
                "name_desc" => studentsQuery.OrderByDescending(s => s.Profile.LastName).ThenByDescending(s => s.Profile.FirstName),
                "code" => studentsQuery.OrderBy(s => s.Student_Code),
                "code_desc" => studentsQuery.OrderByDescending(s => s.Student_Code),
                "gpa" => studentsQuery.OrderBy(s => s.GPA),
                "gpa_desc" => studentsQuery.OrderByDescending(s => s.GPA),
                "date" => studentsQuery.OrderBy(s => s.EnrollmentDate),
                "date_desc" => studentsQuery.OrderByDescending(s => s.EnrollmentDate),
                _ => studentsQuery.OrderBy(s => s.Profile.LastName).ThenBy(s => s.Profile.FirstName),
            };

            int totalItems = await studentsQuery.CountAsync();
            ViewData["TotalItems"] = totalItems;

            return View(await PaginatedList<Student>.CreateAsync(studentsQuery, pageNumber ?? 1, pageSize));
        }

        /*เพิ่มปุ่มให้ตำแหน่งสภานักเรียน*/
        [HttpGet]
        [Route("Student/Details")]
        public async Task<IActionResult> DetailsStudent(int id)
        {
            try
            {
                var student = await _db.Students
                    .Include(s => s.Profile)
                        .ThenInclude(p => p.User)
                    .Include(s => s.Class)
                        .ThenInclude(c => c.GradeLevels)
                    .FirstOrDefaultAsync(s => s.StudentId == id);

                if (student == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบข้อมูลนักเรียน";
                    return RedirectToAction(nameof(IndexStudent));
                }

                // สร้าง view model สำหรับแสดงรายละเอียดนักเรียน
                var model = new StudentViewEditModel
                {
                    StudentId = student.StudentId,
                    Student_Code = student.Student_Code,
                    FirstName = student.Profile.FirstName,
                    LastName = student.Profile.LastName,
                    Gender = student.Profile.Gender,
                    Address = student.Profile.Address,
                    DateOfBirth = student.Profile.DateOfBirth.ToDateTime(new TimeOnly(0, 0)),
                    ProfilePictureUrl = student.Profile.ProfilePictureUrl,
                    EnrollmentDate = student.EnrollmentDate.ToDateTime(new TimeOnly(0, 0)),
                    GPA = student.GPA,
                    Status = student.Status,
                    Email = student.Profile.User?.Email ?? string.Empty,
                    Username = student.Profile.User?.UserName ?? string.Empty,
                    ClassId = student.ClassId,
                    HasStudentCouncilRole = await _db.UserRoles
                        .AnyAsync(ur => ur.UserId == student.Profile.UserId && _roleManager.Roles.Any(r => r.Id == ur.RoleId && r.Name == "StudentCouncil")),
                    // Class display value
                    ClassName = student.Class != null ? $"{student.Class.GradeLevels.Name}/{student.Class.ClassNumber}" : "Not Assigned"
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading student details: {ErrorMessage}", ex.Message);
                TempData["ErrorMessage"] = $"เกิดข้อผิดพลาดในการโหลดรายละเอียดนักเรียน: {ex.Message}";
                return RedirectToAction(nameof(IndexStudent));
            }
        }

        [HttpPost]
        public async Task<IActionResult> AssignStudentCouncilRole(int id)
        {
            var student = await _db.Students
                .Include(t => t.Profile)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(t => t.StudentId == id);

            if (student == null || student.Profile == null || student.Profile.User == null)
            {
                return NotFound("ไม่พบครูหรือผู้ใช้ที่เกี่ยวข้อง");
            }

            var user = student.Profile.User;
            var roleName = "StudentCouncil";

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
            return RedirectToAction("DetailsStudent", new { id });
        }

        [HttpPost]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> RemoveStudentCouncilRole(int id)
        {
            var student = await _db.Students
                .Include(t => t.Profile)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(t => t.StudentId == id);

            if (student == null || student.Profile == null || student.Profile.User == null)
            {
                return NotFound("ไม่พบครูหรือผู้ใช้ที่เกี่ยวข้อง");
            }

            var user = student.Profile.User;
            var roleName = "StudentCouncil";

            var removeResult = await _userManager.RemoveFromRoleAsync(user, roleName);

            if (!removeResult.Succeeded)
            {
                return BadRequest("ไม่สามารถยกเลิกบทบาทได้");
            }

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "ยกเลิกบทบาทสำเร็จ";
            return RedirectToAction("DetailsStudent", new { id });
        }

        [HttpGet]
        [Route("Student/Create")]
        public async Task<IActionResult> CreateStudent()
        {
            // ดึงข้อมูล Class โดย join กับ GradeLevels
            var classList = await _db.Classes
                .Where(g => g.Status == "Active")
                .Include(c => c.GradeLevels)
                .Select(c => new SelectListItem
                {
                    Value = c.ClassId.ToString(),
                    Text = c.GradeLevels != null ? $"{c.GradeLevels.Name}/{c.ClassNumber}" : $"Unknown/{c.ClassNumber}" // Handle possible null reference
                })
                .ToListAsync();

            var model = new StudentViewModel
            {
                Classes = classList
            };

            return View(model);
        }

        [HttpPost]
        [Route("Student/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStudent(StudentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Classes = await _db.Classes
                    .Where(g => g.Status == "Active")
                    .Include(c => c.GradeLevels)
                    .Select(c => new SelectListItem
                    {
                        Value = c.ClassId.ToString(),
                        Text = $"{c.GradeLevels.Name}/{c.ClassNumber}"
                    })
                    .ToListAsync();

                return View(model);
            }

            try
            {

                using var transaction = await _db.Database.BeginTransactionAsync();

                // 1. สร้าง User account
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
                    return View(model);
                }

                // Assign Role (ถ้าจำเป็นสำหรับนักเรียน เช่น "Student")
                await _userManager.AddToRoleAsync(user, "Student");

                // 2. สร้าง Profile
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

                // จัดการการอัพโหลดรูปภาพ
                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    var validationResult = ValidateImageFile(model.ProfilePicture);
                    if (!validationResult.IsValid)
                    {
                        ModelState.AddModelError("ProfilePicture", validationResult.ErrorMessage);
                        model.Classes = await _db.Classes
                            .Where(g => g.Status == "Active")
                            .Include(c => c.GradeLevels)
                            .Select(c => new SelectListItem
                            {
                                Value = c.ClassId.ToString(),
                                Text = $"{c.GradeLevels.Name}/{c.ClassNumber}"
                            })
                            .ToListAsync();
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

                // 3. สร้าง Student โดยอ้างอิงจาก ProfileId ที่ได้
                var student = new Student
                {
                    Student_Code = model.Student_Code,
                    ProfileId = profile.ProfileId,
                    ClassId = model.ClassId,
                    EnrollmentDate = DateOnly.FromDateTime(model.EnrollmentDate),
                    GPA = model.GPA,
                    Status = model.Status,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _db.Students.AddAsync(student);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = "สร้างนักเรียนสำเร็จ!";
                return RedirectToAction(nameof(IndexStudent));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating student: {ErrorMessage}", ex.Message);
                ModelState.AddModelError("", $"Error creating student: {ex.Message}");
                TempData["ErrorMessage"] = $"เกิดข้อผิดพลาดในการสร้างนักเรียน: {ex.Message}";
                return View(model);
            }
        }

        [HttpGet]
        [Route("Student/Edit/{id}")]
        public async Task<IActionResult> EditStudent(int id)
        {
            try
            {
                var student = await _db.Students
                    .Include(s => s.Profile)
                        .ThenInclude(p => p.User)
                    .FirstOrDefaultAsync(s => s.StudentId == id);

                if (student == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบข้อมูลนักเรียน";
                    return RedirectToAction(nameof(IndexStudent));
                }

                var model = new StudentViewEditModel
                {
                    StudentId = student.StudentId,
                    ProfileId = student.ProfileId ?? 0,
                    Student_Code = student.Student_Code,
                    FirstName = student.Profile?.FirstName,
                    LastName = student.Profile?.LastName,
                    Gender = student.Profile?.Gender,
                    Address = student.Profile?.Address,
                    DateOfBirth = student.Profile != null ? student.Profile.DateOfBirth.ToDateTime(TimeOnly.MinValue) : DateTime.MinValue,
                    ProfilePictureUrl = student.Profile?.ProfilePictureUrl, // ส่ง path รูปภาพปัจจุบันไปยัง View
                    ClassId = student.ClassId,
                    EnrollmentDate = student.EnrollmentDate.ToDateTime(new TimeOnly(0, 0)),
                    GPA = student.GPA,
                    Status = student.Status,
                    Email = student.Profile?.User != null ? student.Profile.User.Email : string.Empty,
                    Username = student.Profile?.User != null ? student.Profile.User.UserName : string.Empty,
                    Role = "Student"
                };

                model.Classes = await _db.Classes
                    .Where(g => g.Status == "Active")
                    .Include(c => c.GradeLevels)
                    .Select(c => new SelectListItem
                    {
                        Value = c.ClassId.ToString(),
                        Text = $"{c.GradeLevels.Name}/{c.ClassNumber}"
                    })
                    .ToListAsync();

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading student with id {StudentId}", id);
                TempData["ErrorMessage"] = $"เกิดข้อผิดพลาดในการโหลดรายละเอียดนักเรียน: {ex.Message}";
                return RedirectToAction(nameof(IndexStudent));
            }
        }

        [HttpPost]
        [Route("Student/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStudent(StudentViewEditModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Classes = await _db.Classes
                    .Where(g => g.Status == "Active")
                    .Include(c => c.GradeLevels)
                    .Select(c => new SelectListItem
                    {
                        Value = c.ClassId.ToString(),
                        Text = $"{c.GradeLevels.Name}/{c.ClassNumber}"
                    })
                    .ToListAsync();

                return View(model);
            }

            var student = await _db.Students
                .Include(s => s.Profile)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(s => s.StudentId == model.StudentId);

            if (student == null)
            {
                TempData["ErrorMessage"] = "ไม่พบข้อมูลนักเรียน";
                return RedirectToAction(nameof(IndexStudent));
            }

            try
            {
                // ปรับปรุงข้อมูล Profile
                student.Profile.FirstName = model.FirstName;
                student.Profile.LastName = model.LastName;
                student.Profile.Gender = model.Gender;
                student.Profile.Address = model.Address;
                student.Profile.DateOfBirth = DateOnly.FromDateTime(model.DateOfBirth);

                // จัดการการอัพโหลดรูปภาพใหม่
                if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
                {
                    var validationResult = ValidateImageFile(model.ProfilePicture);
                    if (!validationResult.IsValid)
                    {
                        ModelState.AddModelError("ProfilePicture", validationResult.ErrorMessage);
                        model.Classes = await _db.Classes
                            .Where(g => g.Status == "Active")
                            .Include(c => c.GradeLevels)
                            .Select(c => new SelectListItem
                            {
                                Value = c.ClassId.ToString(),
                                Text = $"{c.GradeLevels.Name}/{c.ClassNumber}"
                            })
                            .ToListAsync();
                        return View(model);
                    }

                    // ลบไฟล์เก่า (ถ้ามี)
                    if (!string.IsNullOrEmpty(student.Profile.ProfilePictureUrl))
                    {
                        var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", student.Profile.ProfilePictureUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // สร้างชื่อไฟล์ใหม่แบบสุ่ม
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePicture.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/profiles", fileName);

                    // บันทึกไฟล์ใหม่
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ProfilePicture.CopyToAsync(stream);
                    }

                    // อัพเดต path ในฐานข้อมูล
                    student.Profile.ProfilePictureUrl = $"/images/profiles/{fileName}";
                }
                // ถ้าไม่มีการอัพโหลดไฟล์ใหม่ คง path เดิมไว้

                // ปรับปรุงข้อมูล Account ของ User (ถ้ามี)
                if (student.Profile.User != null)
                {
                    student.Profile.User.Email = model.Email;
                    student.Profile.User.UserName = model.Username;
                    if (!string.IsNullOrWhiteSpace(model.Password))
                    {
                        var passwordHasher = new PasswordHasher<Users>();
                        student.Profile.User.PasswordHash = passwordHasher.HashPassword(student.Profile.User, model.Password);
                    }
                }

                // ปรับปรุงข้อมูล Student
                student.Student_Code = model.Student_Code;
                student.ClassId = model.ClassId;
                student.EnrollmentDate = DateOnly.FromDateTime(model.EnrollmentDate);
                student.GPA = model.GPA;
                student.Status = model.Status;
                student.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "อัปเดตข้อมูลนักเรียนสำเร็จ!";
                return RedirectToAction(nameof(IndexStudent));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating student with id {StudentId}", model.StudentId);
                TempData["ErrorMessage"] = $"เกิดข้อผิดพลาดในการอัปเดตข้อมูลนักเรียน: {ex.Message}";
                return View(model);
            }
        }

        [HttpPost]
        [Route("Student/Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            try
            {
                var student = await _db.Students
                    .Include(s => s.Profile)
                    .FirstOrDefaultAsync(s => s.StudentId == id);

                if (student == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบข้อมูลนักเรียน";
                    return RedirectToAction(nameof(IndexStudent));
                }

                // ตรวจสอบและลบไฟล์รูปภาพโปรไฟล์ (ถ้ามี)
                if (!string.IsNullOrEmpty(student.Profile?.ProfilePictureUrl))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", student.Profile.ProfilePictureUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // ลบ Student และ Profile ที่เกี่ยวข้อง
                _db.Students.Remove(student);
                _db.Profiles.Remove(student.Profile);
                await _db.SaveChangesAsync();

                // ลบ User ที่เกี่ยวข้องกับ Profile (รวม Role ด้วย) หากมี
                if (!string.IsNullOrEmpty(student.Profile.UserId))
                {
                    var user = await _userManager.FindByIdAsync(student.Profile.UserId);
                    if (user != null)
                    {
                        var result = await _userManager.DeleteAsync(user);
                        if (!result.Succeeded)
                        {
                            TempData["ErrorMessage"] = "การลบผู้ใช้ล้มเหลว";
                            return RedirectToAction(nameof(IndexStudent));
                        }
                    }
                }

                TempData["SuccessMessage"] = "ลบนักเรียนสำเร็จ!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"เกิดข้อผิดพลาดในการลบนักเรียน: {{ex.Message";
            }

            return RedirectToAction(nameof(IndexStudent));
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
                    var image = SixLabors.ImageSharp.Image.Load(stream); // ใช้ SixLabors.ImageSharp
                                                                         // ถ้าโหลดสำเร็จ แปลว่าเป็นภาพ
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
