using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Helpers;
using SchoolSystem.Models.ActivityManagement;
using SchoolSystem.Models.ClassManagement;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSystem.Controllers
{
    [Authorize(Policy = "AdminPolicy")]
    public class SemesterController : Controller
    {
        private readonly AppDbContext _db;

        public SemesterController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Route("Semester")]
        public async Task<IActionResult> IndexSemester(int? pageNumber, string searchString, string sortOrder)
        {
            // เก็บค่าที่ใช้ใน View
            ViewData["CurrentSort"] = sortOrder;
            ViewData["SemesterSortParam"] = string.IsNullOrEmpty(sortOrder) ? "Semester_desc" : "";
            ViewData["CurrentFilter"] = searchString;

            int pageSize = 10;
            var semestersQuery = _db.Semesters.AsNoTracking();

            // ค้นหาโดยดูจาก SemesterYear หรือ SemesterNumber
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                semestersQuery = semestersQuery.Where(s =>
                    (s.SemesterYear + "/" + s.SemesterNumber).Contains(searchString));
            }

            // การจัดเรียงข้อมูล
            semestersQuery = sortOrder switch
            {
                "Semester_desc" => semestersQuery.OrderByDescending(s => s.SemesterYear + "/" + s.SemesterNumber),
                _ => semestersQuery.OrderBy(s => s.SemesterYear + "/" + s.SemesterNumber),
            };

            int totalItems = await semestersQuery.CountAsync();
            ViewData["TotalItems"] = totalItems;

            // ใช้ PaginatedList<Semester> เพื่อแบ่งหน้า
            return View(await PaginatedList<SchoolSystem.Models.ClassManagement.Semester>.CreateAsync(semestersQuery, pageNumber ?? 1, pageSize));
        }


        [HttpGet]
        [Route("Semester/Create")]
        public IActionResult CreateSemester()
        {
            return View();
        }

        [HttpPost]
        [Route("Semester/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateSemester(Semester model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    // บันทึก Semester ใหม่ลงฐานข้อมูล
                    _db.Semesters.Add(model);
                    await _db.SaveChangesAsync();

                    // เฉพาะเมื่อ Semester ที่สร้างขึ้นมีสถานะ Active
                    if (model.Status == "Active")
                    {
                        // ดึง Activity ที่เป็น Daily และมีสถานะ Active
                        var dailyActivityIds = await _db.Activities
                            .Where(a => a.ActivityType == "Daily" && a.Status == "Active")
                            .Select(a => a.ActivityId)
                            .ToListAsync();

                        // สำหรับ Semester ใหม่ เราสร้าง ActivityManagement เฉพาะสำหรับ Activity ที่ Active เท่านั้น
                        var activityManagementList = dailyActivityIds.Select(activityId => new ActivityManagement
                        {
                            ActivityId = activityId,
                            CheckCount = 0,
                            SemesterId = model.SemesterID,
                            Type = "Daily",
                            Status = "Active",
                            UpdatedAt = DateTime.UtcNow,
                            CreatedAt = DateTime.UtcNow
                        }).ToList();

                        if (activityManagementList.Any())
                        {
                            _db.ActivityManagement.AddRange(activityManagementList);
                            await _db.SaveChangesAsync();
                        }
                    }

                    transaction.Commit();
                    TempData["SuccessMessage"] = "Semester created successfully!";
                    return RedirectToAction("IndexSemester");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", $"Error creating semester: {ex.Message}");
                    return View(model);
                }
            }
        }



        // 📌 แสดงฟอร์มแก้ไขภาคเรียน
        [HttpGet]
        [Route("Semester/Edit/{id}")]
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

        [HttpPost]
        [Route("Semester/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditSemester(Semester model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // ใช้ transaction เพื่อให้การอัปเดตทั้งหมดเป็น atomic
            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    // ดึง Semester ที่ต้องการแก้ไข
                    var semester = await _db.Semesters.FirstOrDefaultAsync(s => s.SemesterID == model.SemesterID);
                    if (semester == null)
                    {
                        TempData["ErrorMessage"] = "Semester not found.";
                        return RedirectToAction("IndexSemester");
                    }

                    // อัปเดตข้อมูล Semester
                    semester.SemesterYear = model.SemesterYear;
                    semester.SemesterNumber = model.SemesterNumber;
                    semester.StartTime = model.StartTime;
                    semester.EndTime = model.EndTime;
                    semester.Status = model.Status; // สมมุติว่า Status ถูกแก้ไขได้จาก form
                    semester.UpdatedAt = DateTime.UtcNow;

                    // บันทึกการเปลี่ยนแปลง Semester
                    await _db.SaveChangesAsync();

                    // ดึง Activity ที่เป็น Daily และ Active อยู่ (เฉพาะ ActivityId)
                    var dailyActivities = await _db.Activities
                        .Where(a => a.ActivityType == "Daily" && a.Status == "Active")
                        .Select(a => a.ActivityId)
                        .ToListAsync();

                    // ดึง ActivityManagement ที่มีอยู่แล้วสำหรับ Semester นี้
                    var existingManagements = await _db.ActivityManagement
                        .Where(am => am.SemesterId == semester.SemesterID && dailyActivities.Contains(am.ActivityId))
                        .ToListAsync();

                    // สำหรับแต่ละ Activity ที่เป็น Daily
                    foreach (var activityId in dailyActivities)
                    {
                        var existing = existingManagements.FirstOrDefault(am => am.ActivityId == activityId);
                        if (existing != null)
                        {
                            // ถ้ามีอยู่แล้ว ให้ update Status และ UpdatedAt
                            existing.Status = semester.Status;
                            existing.UpdatedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            // ถ้ายังไม่มี ActivityManagement และ Semester ยัง Active ให้สร้างใหม่
                            if (semester.Status == "Active")
                            {
                                var newManagement = new ActivityManagement
                                {
                                    ActivityId = activityId,
                                    CheckCount = 0,
                                    SemesterId = semester.SemesterID,
                                    Type = "Daily",
                                    Status = semester.Status,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                await _db.ActivityManagement.AddAsync(newManagement);
                            }
                        }
                    }

                    // บันทึกการเปลี่ยนแปลง ActivityManagement
                    await _db.SaveChangesAsync();

                    // ยืนยัน transaction
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Semester updated successfully!";
                    return RedirectToAction("IndexSemester");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", $"Error updating semester: {ex.Message}");
                    return View(model);
                }
            }
        }


        // 📌 ลบภาคเรียนจากหน้า Edit
        [HttpPost]
        [Route("Semester/Delete/{id}")]
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
