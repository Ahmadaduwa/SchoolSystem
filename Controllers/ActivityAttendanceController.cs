using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models.ActivityManagement;
using SchoolSystem.Models.ViewModels;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using SchoolSystem.ViewModels;
using SchoolSystem.Models.Alert;

namespace SchoolSystem.Controllers
{
    [Authorize(Policy = "TeacherPolicy")]
    public class ActivityAttendanceController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<ActivityAttendanceController> _logger;

        public ActivityAttendanceController(AppDbContext db, ILogger<ActivityAttendanceController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        [Route("ActivityAttendance")]
        public async Task<IActionResult> Index()
        {
            var today = DateTime.Now.DayOfWeek;
            if (today == DayOfWeek.Saturday || today == DayOfWeek.Sunday)
            {
                _logger.LogInformation("วันนี้ไม่อยู่ในช่วงวันทำการ (จันทร์-ศุกร์)");
                return View("NoActivity");
            }

            var activities = await _db.ActivityManagement
                .Include(am => am.Activity)
                .Where(am => am.Activity.ActivityType == "Daily")
                .ToListAsync();

            _logger.LogInformation($"พบ {activities.Count} กิจกรรมที่มีประเภท Daily");
            return View(activities);
        }

        [HttpGet]
        public async Task<IActionResult> SelectClass(int activityManagementId)
        {
            var activityManagement = await _db.ActivityManagement
                .Include(am => am.Activity)
                .FirstOrDefaultAsync(am => am.AM_id == activityManagementId);

            if (activityManagement == null)
            {
                _logger.LogWarning($"ไม่พบ ActivityManagementId: {activityManagementId}");
                return NotFound();
            }

            var classes = await _db.Classes
                .Include(c => c.GradeLevels)
                .ToListAsync();

            var viewModel = new ActivityAttendanceSelectClassViewModel
            {
                ActivityManagementId = activityManagementId,
                ActivityName = activityManagement.Activity.ActivityName,
                Classes = classes
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> MarkAttendance(int activityManagementId, int classId)
        {
            var attendanceDate = DateTime.Now.Date;

            var activityManagement = await _db.ActivityManagement
                .Include(am => am.Activity)
                .FirstOrDefaultAsync(am => am.AM_id == activityManagementId);
            if (activityManagement == null)
            {
                _logger.LogWarning($"ActivityManagementId not found: {activityManagementId}");
                return NotFound();
            }

            var students = await _db.Students
                .Include(s => s.Profile)
                .Where(s => s.ClassId == classId)
                .ToListAsync();

            var classInfo = await _db.Classes
                .Include(c => c.GradeLevels)
                .FirstOrDefaultAsync(c => c.ClassId == classId);

            var existingAttendances = await _db.ActivityAttendances
                .Where(a => a.AM_id == activityManagementId
                         && a.TimeStamp == DateOnly.FromDateTime(attendanceDate)
                         && students.Select(s => s.StudentId).Contains(a.Student_id))
                .ToDictionaryAsync(a => a.Student_id, a => a.Status);

            var viewModel = new ActivityAttendanceViewModel
            {
                ActivityManagementId = activityManagementId,
                ActivityName = activityManagement.Activity.ActivityName, // Set here
                AttendanceDate = attendanceDate,
                ClassId = classId,
                Students = students.Select(s => new StudentAttendanceInputModel
                {
                    StudentId = s.StudentId,
                    StudentName = $"{s.Profile?.FirstName} {s.Profile?.LastName}",
                    Status = existingAttendances.TryGetValue(s.StudentId, out var status) ? status : "absent"
                }).ToList()
            };

            ViewBag.ClassName = classInfo != null ? $"{classInfo.GradeLevels?.Name}/{classInfo.ClassNumber}" : "Unknown";
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttendance(ActivityAttendanceViewModel model)
        {
            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    foreach (var error in state.Errors)
                    {
                        _logger.LogWarning($"ModelState error on {key}: {error.ErrorMessage}");
                    }
                }
                return View(model);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt with empty user ID");
                return Unauthorized("Please log in to access this feature");
            }

            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var attendanceDate = DateOnly.FromDateTime(model.AttendanceDate);
                    var amId = model.ActivityManagementId;

                    var activityName = await _db.ActivityManagement
                        .Where(am => am.AM_id == amId)
                        .Select(am => am.Activity.ActivityName)
                        .FirstOrDefaultAsync() ?? "กิจกรรม";

                    var existingAttendanceCheck = await _db.ActivityAttendanceCheck
                        .FirstOrDefaultAsync(aac => aac.AM_Id == amId && aac.Date == attendanceDate);

                    if (existingAttendanceCheck == null)
                    {
                        var attendanceCheck = new ActivityAttendanceCheck
                        {
                            AM_Id = amId,
                            Date = attendanceDate
                        };
                        _db.ActivityAttendanceCheck.Add(attendanceCheck);
                    }

                    foreach (var student in model.Students)
                    {
                        var status = student.Status?.ToLower() ?? "absent";

                        var existingAttendance = await _db.ActivityAttendances
                            .FirstOrDefaultAsync(a => a.AM_id == amId
                                && a.Student_id == student.StudentId
                                && a.TimeStamp == attendanceDate);

                        if (existingAttendance != null)
                        {
                            existingAttendance.Status = status;
                        }
                        else
                        {
                            var newAttendance = new ActivityAttendance
                            {
                                AM_id = amId,
                                Student_id = student.StudentId,
                                Status = status,
                                TimeStamp = attendanceDate
                            };
                            _db.ActivityAttendances.Add(newAttendance);
                        }
                    }
                    await _db.SaveChangesAsync();

                    var checkCount = await _db.ActivityAttendanceCheck
                        .CountAsync(c => c.AM_Id == amId);

                    foreach (var student in model.Students)
                    {
                        var summary = await _db.ActivityAttendanceSummary
                            .FirstOrDefaultAsync(s => s.AM_id == amId && s.StudentId == student.StudentId);

                        if (summary == null)
                        {
                            summary = new ActivityAttendanceSummary
                            {
                                AM_id = amId,
                                StudentId = student.StudentId,
                                PresentCount = 0,
                                AbsentCount = 0,
                                LateCount = 0,
                                ExcusedCount = 0,
                                UpdateAt = DateTime.UtcNow
                            };
                            _db.ActivityAttendanceSummary.Add(summary);
                            await _db.SaveChangesAsync();
                        }

                        summary.PresentCount = await _db.ActivityAttendances
                            .CountAsync(a => a.AM_id == amId && a.Student_id == student.StudentId && a.Status == "present");

                        summary.AbsentCount = await _db.ActivityAttendances
                            .CountAsync(a => a.AM_id == amId && a.Student_id == student.StudentId && a.Status == "absent");

                        summary.LateCount = await _db.ActivityAttendances
                            .CountAsync(a => a.AM_id == amId && a.Student_id == student.StudentId && a.Status == "late");

                        summary.ExcusedCount = await _db.ActivityAttendances
                            .CountAsync(a => a.AM_id == amId && a.Student_id == student.StudentId && a.Status == "excused");

                        summary.UpdateAt = DateTime.UtcNow;

                        var studentProfile = await _db.Students
                            .Include(s => s.Profile)
                            .FirstOrDefaultAsync(s => s.StudentId == student.StudentId);

                        if (studentProfile?.Profile != null)
                        {
                            var profileId = studentProfile.Profile.ProfileId;

                            var effectiveAbsenceCount = summary.AbsentCount + (Math.Floor((double)summary.LateCount / 3));
                            var totalChecks = checkCount; 
                            var absenceRate = (totalChecks > 5) ? (effectiveAbsenceCount / totalChecks) * 100 : 0;

          
                            if (absenceRate > 30)
                            {
                                string notificationType = $"{amId}/ActivityAbsenceWarning";

                                var existingNotification = await _db.Notifications
                                    .FirstOrDefaultAsync(n => n.ProfileId == profileId && n.NotificationType == notificationType);

                                if (existingNotification == null)
                                {
               
                                    var notification = new Notification
                                    {
                                        NotificationType = notificationType,
                                        NotificationTime = DateTime.UtcNow,
                                        Message = $"คำเตือน: คุณขาดกิจกรรม {activityName} เกิน 30% โดยมีอัตราการขาดกิจกรรมที่ {absenceRate:F1}%",
                                        ProfileId = profileId,
                                        Status = "Pending"
                                    };

                                    _db.Notifications.Add(notification);
                                }
                                else
                                {
                                    // Update existing notification
                                    existingNotification.NotificationTime = DateTime.UtcNow;
                                    existingNotification.Message = $"คำเตือน: คุณขาดกิจกรรม {activityName} เกิน 30% โดยมีอัตราการขาดกิจกรรมที่ {absenceRate:F1}%";
                                    existingNotification.Status = "Pending";
                                }
                            }

                            // Check if student has late attendances that form a multiple of 3
                            if (summary.LateCount >= 3)
                            {
                                // Create a unique notification type with AMID and late count milestone
                                int lateMilestone = (int)Math.Floor((double)summary.LateCount / 3) * 3; // Gets 3, 6, 9, etc.
                                string notificationType = $"{amId}/ActivityLateWarning/{lateMilestone}";

                                // Check if a notification of this type already exists for this student
                                var existingNotification = await _db.Notifications
                                    .FirstOrDefaultAsync(n => n.ProfileId == profileId && n.NotificationType == notificationType);

                                if (existingNotification == null && summary.LateCount % 3 == 0)
                                {
                                    // Create new notification for late attendance
                                    var lateEquivalentToAbsence = summary.LateCount / 3;
                                    var notification = new Notification
                                    {
                                        NotificationType = notificationType,
                                        NotificationTime = DateTime.UtcNow,
                                        Message = $"คำเตือน: คุณมาสายในกิจกรรม {activityName} จำนวน {summary.LateCount} ครั้ง ซึ่งเทียบเท่ากับขาดกิจกรรม {lateEquivalentToAbsence} ครั้ง",
                                        ProfileId = profileId,
                                        Status = "Pending"
                                    };

                                    _db.Notifications.Add(notification);
                                }
                            }
                        }
                    }

                    // Update activity check count
                    var activityManagement = await _db.ActivityManagement
                        .FirstOrDefaultAsync(am => am.AM_id == amId);
                    if (activityManagement != null)
                    {
                        activityManagement.CheckCount = checkCount;
                    }

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation($"Attendance marked/updated successfully for ActivityManagementId: {amId} on date: {attendanceDate}");
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error while marking/updating activity attendance");
                    return StatusCode(500, "An error occurred while saving attendance data");
                }
            }
        }
    }
}