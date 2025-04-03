using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Helpers;
using SchoolSystem.Models.ActivityManagement;

namespace SchoolSystem.Controllers
{
    [Authorize(Policy = "AdminPolicy")]
    public class ActivityController : Controller
    {
        private readonly AppDbContext _db;

        public ActivityController(AppDbContext db)
        {
            _db = db;
        }
        [HttpGet]
        [Route("Activity")]
        public async Task<IActionResult> IndexActivity(int? pageNumber, string searchString, string sortOrder, string activityTypeFilter)
        {
            // เก็บค่าเพื่อใช้งานใน View
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["TypeSortParam"] = sortOrder == "type" ? "type_desc" : "type";
            ViewData["StatusSortParam"] = sortOrder == "status" ? "status_desc" : "status";
            ViewData["DateSortParam"] = sortOrder == "date" ? "date_desc" : "date";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentTypeFilter"] = activityTypeFilter;

            // สร้างรายการสำหรับ Dropdown กรองตามประเภทกิจกรรม
            var activityTypes = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "All Types" },
                    new SelectListItem { Value = "Daily", Text = "Daily" },
                    new SelectListItem { Value = "Special", Text = "Special" }
                };
            ViewData["ActivityTypes"] = activityTypes;

            int pageSize = 10;
            var activitiesQuery = _db.Activities.AsNoTracking();

            // ค้นหาตามชื่อกิจกรรมหรือคำอธิบาย
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                activitiesQuery = activitiesQuery.Where(a =>
                    a.ActivityName.Contains(searchString) ||
                    a.Description.Contains(searchString));
            }

            // กรองตามประเภทกิจกรรม
            if (!string.IsNullOrWhiteSpace(activityTypeFilter))
            {
                activitiesQuery = activitiesQuery.Where(a => a.ActivityType == activityTypeFilter);
            }

            // การจัดเรียงข้อมูล
            activitiesQuery = sortOrder switch
            {
                "name_desc" => activitiesQuery.OrderByDescending(a => a.ActivityName),
                "type" => activitiesQuery.OrderBy(a => a.ActivityType),
                "type_desc" => activitiesQuery.OrderByDescending(a => a.ActivityType),
                "status" => activitiesQuery.OrderBy(a => a.Status),
                "status_desc" => activitiesQuery.OrderByDescending(a => a.Status),
                "date" => activitiesQuery.OrderBy(a => a.CreateAt),
                "date_desc" => activitiesQuery.OrderByDescending(a => a.CreateAt),
                _ => activitiesQuery.OrderBy(a => a.ActivityName),
            };

            int totalItems = await activitiesQuery.CountAsync();
            ViewData["TotalItems"] = totalItems;

            // ใช้ PaginatedList<Activity> ที่มีอยู่แล้วในระบบ
            return View(await PaginatedList<SchoolSystem.Models.ActivityManagement.Activity>.CreateAsync(activitiesQuery, pageNumber ?? 1, pageSize));
        }


        // เพิ่มกิจกรรม
        [HttpGet]
        [Route("Activity/Create")]
        public IActionResult CreateActivity()
        {
            return View();
        }

        [HttpPost]
        [Route("Activity/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateActivity(Activity newActivity, bool isDaily)
        {
            if (!ModelState.IsValid)
            {
                return View(newActivity);
            }

            // ใช้ transaction เพื่อให้การบันทึกทั้งหมดเป็น atomic operation
            using (var transaction = _db.Database.BeginTransaction())
            {
                try
                {
                    // บันทึก Activity ใหม่ลงฐานข้อมูล
                    _db.Activities.Add(newActivity);
                    await _db.SaveChangesAsync();

                    // สร้าง ActivityManagement เฉพาะเมื่อเป็น Daily และ Activity มีสถานะ Active
                    if (isDaily && newActivity.Status == "Active")
                    {
                        // ดึงเฉพาะ Semester ที่ Active
                        var activeSemesterIds = await _db.Semesters
                            .Where(s => s.Status == "Active")
                            .Select(s => s.SemesterID)
                            .ToListAsync();

                        // สร้าง ActivityManagement สำหรับแต่ละ Semester ที่ Active
                        var activityManagementList = activeSemesterIds.Select(semesterId => new ActivityManagement
                        {
                            ActivityId = newActivity.ActivityId,
                            CheckCount = 0,
                            SemesterId = semesterId,
                            Type = "Daily",
                            Status = "Active", // กำหนดเป็น Active เพราะทั้ง Activity กับ Semester Active
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
                    TempData["SuccessMessage"] = "Activity created successfully!";
                    return RedirectToAction("IndexActivity");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", $"Database Error: {ex.Message}");
                    return View(newActivity);
                }
            }
        }


        // แสดงหน้าแก้ไขกิจกรรม
        [HttpGet]
        [Route("Activity/Edit/{id}")]
        public IActionResult EditActivity(int id)
        {
            var activityToEdit = _db.Activities.Find(id);
            if (activityToEdit == null)
            {
                return NotFound();
            }
            return View(activityToEdit);
        }
        [HttpPost]
        [Route("Activity/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditActivity(Activity updatedActivity)
        {
            if (!ModelState.IsValid)
            {
                return View(updatedActivity);
            }

            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    // ดึงรายการ Semester ที่มี Status = "Active"
                    var activeSemesterIds = await _db.Semesters
                        .Where(s => s.Status == "Active")
                        .Select(s => s.SemesterID)
                        .ToListAsync();

                    // ดึง ActivityManagement ที่เกี่ยวข้องกับ Activity นี้ใน Semester ที่ Active
                    var existingManagements = await _db.ActivityManagement
                        .Where(am => am.ActivityId == updatedActivity.ActivityId && activeSemesterIds.Contains(am.SemesterId))
                        .ToListAsync();

                    // สำหรับแต่ละ Semester ที่ Active
                    foreach (var semesterId in activeSemesterIds)
                    {
                        var existing = existingManagements.FirstOrDefault(am => am.SemesterId == semesterId);
                        if (existing != null)
                        {
                            // ถ้ามีอยู่แล้ว ให้ update Status และ UpdatedAt
                            existing.Status = updatedActivity.Status;
                            existing.UpdatedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            // ถ้ายังไม่มี ActivityManagement และ Activity ยัง Active ให้สร้างใหม่
                            if (updatedActivity.Status == "Active")
                            {
                                var newManagement = new ActivityManagement
                                {
                                    ActivityId = updatedActivity.ActivityId,
                                    CheckCount = 0,
                                    SemesterId = semesterId,
                                    Type = "Daily",
                                    Status = updatedActivity.Status,
                                    CreatedAt = DateTime.UtcNow,
                                    UpdatedAt = DateTime.UtcNow
                                };
                                await _db.ActivityManagement.AddAsync(newManagement);
                            }
                        }
                    }

                    // บันทึกการเปลี่ยนแปลง ActivityManagement
                    await _db.SaveChangesAsync();

                    // อัปเดตข้อมูล Activity
                    _db.Activities.Update(updatedActivity);
                    await _db.SaveChangesAsync();

                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Activity updated successfully!";
                    return RedirectToAction("IndexActivity");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                    return View(updatedActivity);
                }
            }
        }


        [HttpPost]
        [Route("Activity/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteActivity(int id)
        {
            try
            {
                var activityToDelete = _db.Activities
                    .FirstOrDefault(a => a.ActivityId == id);

                if (activityToDelete == null)
                {
                    TempData["ErrorMessage"] = "Activity not found.";
                    return RedirectToAction("IndexActivity");
                }

                _db.Activities.Remove(activityToDelete);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Activity deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting activity: {ex.Message}";
            }

            return RedirectToAction("IndexActivity");
        }
    }
}
