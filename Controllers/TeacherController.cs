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

namespace SchoolSystem.Controllers
{
    public class TeacherController : Controller
    {
        private readonly AppDbContext _db;

        public TeacherController(AppDbContext db)
        {
            _db = db;
        }

        // 📌 แสดงรายชื่อครู
        public async Task<IActionResult> IndexTeacher(int? pageNumber)
        {
            int pageSize = 10;
            var teachers = _db.Teachers
                .Include(t => t.Profile)
                .OrderByDescending(t => t.UpdateAt)
                .AsNoTracking();

            return View(await PaginatedList<Teacher>.CreateAsync(teachers, pageNumber ?? 1, pageSize));
        }

        // 📌 ดูรายละเอียดครู
        public async Task<IActionResult> DetailsTeacher(int id)
        {
            try
            {
                var teacher = await _db.Teachers
                    .Include(t => t.Profile) // โหลด Profile มาพร้อมกับ Teacher
                    .FirstOrDefaultAsync(t => t.TeacherId == id);

                if (teacher == null)
                {
                    TempData["ErrorMessage"] = "Teacher not found.";
                    return RedirectToAction("IndexTeacher");
                }

                return View(teacher);
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
                model.Departments = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "Select Department (Optional)" },
            new SelectListItem { Value = "1", Text = "Mathematics" },
            new SelectListItem { Value = "2", Text = "Science" },
            new SelectListItem { Value = "3", Text = "English" },
            new SelectListItem { Value = "4", Text = "History" },
            new SelectListItem { Value = "5", Text = "Computer Science" }
        };

                return View(model);
            }

            try
            {
                using var transaction = await _db.Database.BeginTransactionAsync(); // ✅ ใช้ Transaction ป้องกันปัญหา

                // ✅ 1. สร้าง Profile และ Teacher พร้อมกัน
                var profile = new Profiles
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Gender = model.Gender,
                    Address = model.Address,
                    DateOfBirth = DateOnly.FromDateTime(model.DateOfBirth),
                    ProfilePictureUrl = "", // ป้องกัน NULL Error
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var teacher = new Teacher
                {
                    Profile = profile, // ✅ เชื่อม Profile ตรงนี้เลย
                    DepartmentId = model.DepartmentId ?? 0,
                    HireDate = model.HireDate,
                    Salary = model.Salary,
                    Status = model.Status,
                    CreateAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow
                };

                // ✅ บันทึกข้อมูลลงฐานข้อมูล
                await _db.Teachers.AddAsync(teacher);
                await _db.SaveChangesAsync();

                await transaction.CommitAsync(); // ✅ บันทึก Transaction ถ้าทุกอย่างถูกต้อง

                TempData["SuccessMessage"] = "Teacher created successfully!";
                return RedirectToAction("IndexTeacher");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating teacher: {ex.Message}");
                return View(model);
            }
        }






        [HttpGet]
        public async Task<IActionResult> EditTeacher(int id)
        {
            try
            {
                var teacher = await _db.Teachers
                    .Include(t => t.Profile)
                    .FirstOrDefaultAsync(t => t.TeacherId == id);

                if (teacher == null)
                {
                    TempData["ErrorMessage"] = "Teacher not found.";
                    return RedirectToAction("IndexTeacher");
                }

                return View(teacher);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading teacher: {ex.Message}";
                return RedirectToAction("IndexTeacher");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTeacher(Teacher model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var teacher = await _db.Teachers
                    .Include(t => t.Profile)
                    .FirstOrDefaultAsync(t => t.TeacherId == model.TeacherId);

                if (teacher == null)
                {
                    TempData["ErrorMessage"] = "Teacher not found.";
                    return RedirectToAction("IndexTeacher");
                }

                // ✅ อัปเดตข้อมูล Profile
                teacher.Profile.FirstName = model.Profile.FirstName;
                teacher.Profile.LastName = model.Profile.LastName;
                teacher.Profile.Gender = model.Profile.Gender;
                teacher.Profile.Address = model.Profile.Address;
                teacher.Profile.DateOfBirth = model.Profile.DateOfBirth;

                // ✅ อัปเดตข้อมูล Teacher
                teacher.DepartmentId = model.DepartmentId;
                teacher.HireDate = model.HireDate;
                teacher.Salary = model.Salary;
                teacher.UpdateAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Teacher updated successfully!";
                return RedirectToAction("IndexTeacher");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating teacher: {ex.Message}");
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
                    TempData["ErrorMessage"] = "Teacher not found.";
                    return RedirectToAction("IndexTeacher");
                }

                // ✅ ลบ Profile และ Teacher ไปพร้อมกัน
                _db.Teachers.Remove(teacher);
                _db.Profiles.Remove(teacher.Profile);
                await _db.SaveChangesAsync();

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
