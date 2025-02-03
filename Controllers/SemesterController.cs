using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models.ClassManagement;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSystem.Controllers
{
    public class SemesterController : Controller
    {
        private readonly AppDbContext _db;

        public SemesterController(AppDbContext db)
        {
            _db = db;
        }

        // 📌 แสดงรายการภาคเรียนทั้งหมด
        public async Task<IActionResult> IndexSemester()
        {
            try
            {
                var semesters = await _db.Semesters
                    .AsNoTracking()
                    .OrderByDescending(s => s.SemesterYear)
                    .ThenByDescending(s => s.SemesterNumber)
                    .ToListAsync();
                return View(semesters);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading semesters: {ex.Message}";
                return View(new List<Semester>());
            }
        }

        // 📌 แสดงฟอร์มสร้างภาคเรียน
        public IActionResult CreateSemester()
        {
            return View();
        }

        // 📌 บันทึกการสร้างภาคเรียน
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSemester(Semester model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                _db.Semesters.Add(model);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Semester created successfully!";
                return RedirectToAction("IndexSemester");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating semester: {ex.Message}");
                return View(model);
            }
        }

        // 📌 แสดงฟอร์มแก้ไขภาคเรียน
        public async Task<IActionResult> EditSemester(int id)
        {
            try
            {
                var semester = await _db.Semesters.FirstOrDefaultAsync(s => s.SemesterID == id);
                if (semester == null)
                {
                    TempData["ErrorMessage"] = "Semester not found.";
                    return RedirectToAction("IndexSemester");
                }
                return View(semester);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error loading semester: {ex.Message}";
                return RedirectToAction("IndexSemester");
            }
        }

        // 📌 บันทึกการแก้ไขภาคเรียน
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSemester(Semester model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var semester = await _db.Semesters.FirstOrDefaultAsync(s => s.SemesterID == model.SemesterID);
                if (semester == null)
                {
                    TempData["ErrorMessage"] = "Semester not found.";
                    return RedirectToAction("IndexSemester");
                }

                semester.SemesterYear = model.SemesterYear;
                semester.SemesterNumber = model.SemesterNumber;
                semester.StartTime = model.StartTime;
                semester.EndTime = model.EndTime;

                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Semester updated successfully!";
                return RedirectToAction("IndexSemester");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating semester: {ex.Message}");
                return View(model);
            }
        }

        // 📌 ลบภาคเรียนจากหน้า Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSemester(int id)
        {
            try
            {
                var semester = await _db.Semesters
                    .Include(s => s.ClassManagements) // ตรวจสอบว่าเชื่อมโยงกับข้อมูลอื่นหรือไม่
                    .FirstOrDefaultAsync(s => s.SemesterID == id);

                if (semester == null)
                {
                    TempData["ErrorMessage"] = "Semester not found.";
                    return RedirectToAction("IndexSemester");
                }

                // ตรวจสอบว่ามีคลาสที่เชื่อมโยงกับภาคเรียนนี้หรือไม่
                if (semester.ClassManagements.Any())
                {
                    TempData["ErrorMessage"] = "Cannot delete semester because it is associated with existing classes.";
                    return RedirectToAction("IndexSemester");
                }

                _db.Semesters.Remove(semester);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Semester deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting semester: {ex.Message}";
            }
            return RedirectToAction("IndexSemester");
        }
    }
}
