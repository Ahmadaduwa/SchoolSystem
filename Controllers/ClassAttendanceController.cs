using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models.ViewModels;
using Microsoft.Extensions.Logging;
using SchoolSystem.ViewModels;
using SchoolSystem.Models.ClassManagement;
using SchoolSystem.Models.Alert;

namespace SchoolSystem.Controllers
{
    [Authorize(Policy = "TeacherPolicy")]
    public class ClassAttendanceController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<ClassAttendanceController> _logger;

        public ClassAttendanceController(AppDbContext db, ILogger<ClassAttendanceController> logger)
        {
            _db = db;
            _logger = logger;
        }

        private async Task<int?> GetLoggedInTeacherIdAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt with empty user ID");
                return null;
            }

            var teacherInfo = await _db.Users
                .Where(u => u.Id == userId)
                .Select(u => u.Profile!.Teacher!.TeacherId)
                .FirstOrDefaultAsync();

            return teacherInfo == 0 ? null : teacherInfo;
        }
        private string GetThaiDay(DateTime date)
        {
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Sunday: return "อาทิตย์";
                case DayOfWeek.Monday: return "จันทร์";
                case DayOfWeek.Tuesday: return "อังคาร";
                case DayOfWeek.Wednesday: return "พุธ";
                case DayOfWeek.Thursday: return "พฤหัสบดี";
                case DayOfWeek.Friday: return "ศุกร์";
                case DayOfWeek.Saturday: return "เสาร์";
                default: return "";
            }
        }

        [HttpGet]
        public async Task<IActionResult> ClassAttendance()
        {
            try
            {
                var teacherId = await GetLoggedInTeacherIdAsync();
                if (teacherId == null)
                {
                    return Unauthorized("กรุณาเข้าสู่ระบบเพื่อเข้าถึงข้อมูล");
                }
                _logger.LogInformation($"Found teacher ID: {teacherId}");

                var culture = new System.Globalization.CultureInfo("en-US");
                string currentDay = DateTime.Now.ToString("dddd", culture);
                _logger.LogInformation($"Current day: {currentDay}");

                // ตรวจสอบตารางทั้งหมดของครู (สำหรับ Debug)
                var allTeacherSchedulesCount = await _db.ClassSchedules
                    .Where(cs => cs.ClassManagement.TeacherId == teacherId && cs.Status == "Active" && cs.ClassManagement.Status == "Active")
                    .CountAsync();
                _logger.LogInformation($"Total schedules for teacher: {allTeacherSchedulesCount}");

                // ดึงตารางเรียนของวันนี้โดยเรียงตามเวลาเริ่มต้น
                var schedules = await _db.ClassSchedules
                    .Where(cs => cs.ClassManagement.TeacherId == teacherId && cs.DayOfWeek == currentDay && cs.Status == "Active" && cs.ClassManagement.Status == "Active")
                    .OrderBy(cs => cs.StartTime)
                    .Select(cs => new ClassScheduleViewModel
                    {
                        Id = cs.CM_ID,
                        Name = cs.ClassManagement.Course.CourseName ?? "ไม่ระบุชื่อวิชา",
                        Time = string.Format("{0:hh\\:mm}", cs.StartTime) + " น.",
                        ClassId = cs.ClassManagement.ClassId,
                        CourseId = cs.ClassManagement.CourseId,
                        SemesterId = cs.ClassManagement.SemesterId,
                        TeacherId = cs.ClassManagement.TeacherId,
                        ClassNumber = cs.ClassManagement.Class!.ClassNumber,
                        GradeLevel = cs.ClassManagement.Class!.GradeLevels!.Name ?? "ไม่ระบุระดับชั้น",
                        CourseCode = cs.ClassManagement.Course!.Course_Code ?? "ไม่มีรหัสวิชา",
                    })
                    .ToListAsync();

                _logger.LogInformation($"Found {schedules.Count} schedules for today");

                // สร้าง ViewModel สำหรับหน้า ClassAttendance
                var viewModel = new ClassAttendanceViewModel
                {
                    Schedules = schedules,
                    CurrentDay = GetThaiDay(DateTime.Now),
                    CurrentDate = DateTime.Now.ToString("dd MMMM yyyy", new System.Globalization.CultureInfo("th-TH")),
                    TeacherId = teacherId.Value,
                    Debug_AllTeacherSchedulesCount = allTeacherSchedulesCount
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving class attendance data");
                return StatusCode(500, "เกิดข้อผิดพลาดในการดึงข้อมูลตารางเรียน");
            }
        }

        [HttpGet]
        public async Task<IActionResult> MarkAttendance(int cmId, DateTime? date)
        {
            // ถ้าไม่มีวันที่ระบุ ใช้วันที่ปัจจุบัน
            var attendanceDate = date.HasValue ? DateOnly.FromDateTime(date.Value.Date) : DateOnly.FromDateTime(DateTime.Now.Date);

            // ดึงข้อมูล Class Management พร้อม Course และ Class
            var classManagement = await _db.ClassManagements
                .Where(cs => cs.Status == "Active")
                .Include(cm => cm.Class)
                .ThenInclude(cm => cm.GradeLevels)
                .Include(cm => cm.Course)
                .FirstOrDefaultAsync(cm => cm.CM_Id == cmId);
            if (classManagement == null)
            {
                return NotFound();
            }

            // ดึงนักเรียนในคลาสนั้น
            var studentsInClass = await _db.Students
                .Include(s => s.Profile)
                .Where(s => s.ClassId == classManagement.ClassId)
                .ToListAsync();

            // ดึงข้อมูลการเช็คชื่อที่มีอยู่แล้วสำหรับวันนี้
            var existingAttendance = await _db.ClassAttendance
                .Where(a => a.CM_Id == cmId && a.Date == attendanceDate)
                .ToListAsync();

            var studentAttendanceList = studentsInClass.Select(s => new StudentAttendanceInputModel
            {
                StudentId = s.StudentId,
                StudentName = $"{s.Profile.FirstName} {s.Profile.LastName}",
                Status = "absent"
            }).ToList();

            // อัปเดตสถานะจากข้อมูลที่มีอยู่แล้ว
            foreach (var attendance in existingAttendance)
            {
                var student = studentAttendanceList.FirstOrDefault(s => s.StudentId == attendance.StudentId);
                if (student != null)
                {
                    student.Status = attendance.Status;
                }
            }

            var model = new AttendanceViewModel
            {
                cmId = cmId,
                date = attendanceDate,
                ClassName = classManagement.Class?.GradeLevels.Name+"/"+classManagement.Class?.ClassNumber.ToString(),
                CourseName = classManagement.Course?.CourseName,
                Students = studentAttendanceList
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAttendance(AttendanceViewModel model)
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
                TempData["ErrorMessage"] = "Model validation failed. Please correct the errors and try again.";
                return View(model);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("Unauthorized access attempt with empty user ID");
                TempData["ErrorMessage"] = "Unauthorized access. Please log in to access this information.";
                return Unauthorized("กรุณาเข้าสู่ระบบเพื่อเข้าถึงข้อมูล");
            }

            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var attendanceDate = model.date;
                    var cmId = model.cmId;

                    // Check if attendance has already been marked for this date
                    var existingAttendanceCheck = await _db.ClassAttendanceCheck
                        .FirstOrDefaultAsync(cac => cac.CM_Id == cmId && cac.Date == attendanceDate);

                    if (existingAttendanceCheck == null)
                    {
                        var attendanceCheck = new ClassAttendanceCheck
                        {
                            CM_Id = cmId,
                            Date = attendanceDate
                        };
                        _db.ClassAttendanceCheck.Add(attendanceCheck);
                    }

                    // Handle individual student attendance records
                    foreach (var student in model.Students)
                    {
                        var existingAttendanceRecord = await _db.ClassAttendance
                            .FirstOrDefaultAsync(a => a.CM_Id == cmId && a.StudentId == student.StudentId && a.Date == attendanceDate);

                        if (existingAttendanceRecord != null)
                        {
                            existingAttendanceRecord.Status = student.Status;
                        }
                        else
                        {
                            var attendanceRecord = new ClassAttendance
                            {
                                CM_Id = cmId,
                                StudentId = student.StudentId,
                                Status = student.Status,
                                Date = attendanceDate
                            };
                            _db.ClassAttendance.Add(attendanceRecord);
                        }
                    }
                    await _db.SaveChangesAsync();

                    // Get class information for notifications
                    var classManagement = await _db.ClassManagements
                        .Where(cs => cs.Status == "Active")
                        .Include(cm => cm.Course)
                        .FirstOrDefaultAsync(cm => cm.CM_Id == cmId);

                    if (classManagement == null)
                    {
                        throw new Exception("Class management information not found");
                    }

                    // Update check count in ClassManagement
                    var checkCount = await _db.ClassAttendanceCheck
                        .CountAsync(c => c.CM_Id == cmId);

                    classManagement.CheckCount = checkCount;

                    var courseName = classManagement.Course?.CourseName ?? "ไม่ระบุชื่อ";

                    // Handle attendance summaries and create notifications inline
                    foreach (var student in model.Students)
                    {
                        var summary = await _db.ClassAttendanceSummary
                            .FirstOrDefaultAsync(s => s.CM_Id == cmId && s.StudentId == student.StudentId);

                        if (summary == null)
                        {
                            // Create and add new summary
                            summary = new ClassAttendanceSummary
                            {
                                CM_Id = cmId,
                                StudentId = student.StudentId,
                                PresentCount = 0,
                                AbsentCount = 0,
                                LateCount = 0,
                                ExcusedCount = 0,
                                UpdateAt = DateTime.UtcNow
                            };
                            _db.ClassAttendanceSummary.Add(summary);
                            await _db.SaveChangesAsync(); // Save to generate SAM_Id
                        }

                        // Update counts
                        summary.PresentCount = await _db.ClassAttendance
                            .CountAsync(a => a.CM_Id == cmId && a.StudentId == student.StudentId && a.Status.ToLower() == "present");

                        summary.AbsentCount = await _db.ClassAttendance
                            .CountAsync(a => a.CM_Id == cmId && a.StudentId == student.StudentId && a.Status.ToLower() == "absent");

                        summary.LateCount = await _db.ClassAttendance
                            .CountAsync(a => a.CM_Id == cmId && a.StudentId == student.StudentId && a.Status.ToLower() == "late");

                        summary.ExcusedCount = await _db.ClassAttendance
                            .CountAsync(a => a.CM_Id == cmId && a.StudentId == student.StudentId && a.Status.ToLower() == "excused");

                        summary.UpdateAt = DateTime.UtcNow;

                        // INLINE NOTIFICATION CHECK - After counts are updated for each student
                        // Fetch student profile for notification
                        var studentProfile = await _db.Students
                            .Include(s => s.Profile)
                            .FirstOrDefaultAsync(s => s.StudentId == student.StudentId);

                        if (studentProfile?.Profile != null)
                        {
                            var profileId = studentProfile.Profile.ProfileId;

                            var effectiveAbsenceCount = summary.AbsentCount + (Math.Floor((double)summary.LateCount / 3));
                            var totalClasses = checkCount;
                            var absenceRate = (totalClasses > 5) ? (effectiveAbsenceCount / totalClasses) * 100 : 0;

                            if (absenceRate > 30)
                            {
                                string notificationType = $"{cmId}/AbsenceWarning";

                                var existingNotification = await _db.Notifications
                                    .FirstOrDefaultAsync(n => n.ProfileId == profileId && n.NotificationType == notificationType);

                                if (existingNotification == null)
                                {
                                    var notification = new Notification
                                    {
                                        NotificationType = notificationType,
                                        NotificationTime = DateTime.UtcNow,
                                        Message = $"คำเตือน: คุณขาดเรียนในวิชา {courseName} เกิน 30% โดยมีอัตราการขาดเรียนที่ {absenceRate:F1}%",
                                        ProfileId = profileId,
                                        Status = "Pending"
                                    };

                                    _db.Notifications.Add(notification);
                                }
                                else
                                {
                                    existingNotification.NotificationTime = DateTime.UtcNow;
                                    existingNotification.Message = $"คำเตือน: คุณขาดเรียนในวิชา {courseName} เกิน 30% โดยมีอัตราการขาดเรียนที่ {absenceRate:F1}%";
                                    existingNotification.Status = "Pending";
                                }
                            }

                            if (summary.LateCount >= 3)
                            {
                                int lateMilestone = (int)Math.Floor((double)summary.LateCount / 3) * 3;
                                string notificationType = $"{cmId}/LateWarning/{lateMilestone}";

                                var existingNotification = await _db.Notifications
                                    .FirstOrDefaultAsync(n => n.ProfileId == profileId && n.NotificationType == notificationType);

                                if (existingNotification == null && summary.LateCount % 3 == 0)
                                {
                                    var lateEquivalentToAbsence = summary.LateCount / 3;
                                    var notification = new Notification
                                    {
                                        NotificationType = notificationType,
                                        NotificationTime = DateTime.UtcNow,
                                        Message = $"คำเตือน: คุณมาสายในวิชา {courseName} จำนวน {summary.LateCount} ครั้ง ซึ่งเทียบเท่ากับขาดเรียน {lateEquivalentToAbsence} ครั้ง",
                                        ProfileId = profileId,
                                        Status = "Pending"
                                    };

                                    _db.Notifications.Add(notification);
                                }
                            }
                        }
                    }

                    await _db.SaveChangesAsync(); // Save all summaries and notifications at once
                    await transaction.CommitAsync();

                    _logger.LogInformation($"Attendance marked/updated successfully for CM_Id: {cmId} on date: {attendanceDate}");
                    TempData["SuccessMessage"] = "Attendance marked/updated successfully!";
                    return RedirectToAction("ClassAttendance", "ClassAttendance");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error while marking/updating attendance");
                    TempData["ErrorMessage"] = "An error occurred while marking/updating attendance. Please try again.";
                    return StatusCode(500, "เกิดข้อผิดพลาดในการบันทึกข้อมูลการเช็คชื่อ");
                }
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditAttendanceSelectDate(int cmId)
        {
            // ตรวจสอบว่า cmId อยู่ในเทอมปัจจุบันหรือไม่
            var currentSemester = await _db.Semesters
                .Where(s => DateTime.Now >= s.StartTime && DateTime.Now <= s.EndTime)
                .FirstOrDefaultAsync();
            if (currentSemester == null)
            {
                return NotFound("ไม่พบเทอมปัจจุบันในระบบ");
            }

            var classManagement = await _db.ClassManagements
                .Where(cs => cs.Status == "Active")
                .Include(cm => cm.Class)
                .ThenInclude(cm => cm.GradeLevels)
                .Include(cm => cm.Course)
                .FirstOrDefaultAsync(cm => cm.CM_Id == cmId);
            if (classManagement == null)
            {
                return NotFound("ไม่พบข้อมูลคลาส");
            }
            if (classManagement.SemesterId != currentSemester.SemesterID)
            {
                return BadRequest("ไม่สามารถแก้ไขการเช็คชื่อของคลาสที่ไม่ใช่เทอมปัจจุบันได้");
            }

            // ดึงวันที่ที่เคยเช็คชื่อแล้ว
            var checkedDates = await _db.ClassAttendanceCheck
                .Where(c => c.CM_Id == cmId)
                .OrderByDescending(c => c.Date)
                .Select(c => c.Date)
                .ToListAsync();

            var model = new EditAttendanceSelectDateViewModel
            {
                CM_Id = cmId,
                ClassName = $"{classManagement.Class?.GradeLevels?.Name}/{classManagement.Class?.ClassNumber}",
                CourseName = classManagement.Course?.CourseName,
                CheckedDates = checkedDates
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditAttendance(int cmId, DateOnly date)
        {
            var currentSemester = await _db.Semesters
                .Where(s => DateTime.Now >= s.StartTime && DateTime.Now <= s.EndTime)
                .FirstOrDefaultAsync();
            if (currentSemester == null)
            {
                return NotFound("ไม่พบเทอมปัจจุบันในระบบ");
            }

            var classManagement = await _db.ClassManagements
                .Where(cs => cs.Status == "Active")
                .Include(cm => cm.Class)
                .ThenInclude(c => c.GradeLevels)
                .Include(cm => cm.Course)
                .FirstOrDefaultAsync(cm => cm.CM_Id == cmId);
            if (classManagement == null)
            {
                return NotFound("ไม่พบข้อมูลคลาส");
            }
            if (classManagement.SemesterId != currentSemester.SemesterID)
            {
                return BadRequest("ไม่สามารถแก้ไขการเช็คชื่อของคลาสที่ไม่ใช่เทอมปัจจุบันได้");
            }

            // ตรวจสอบว่ามีการเช็คชื่อในวันที่เลือกหรือไม่
            var dateOnly = date;
            var existingCheck = await _db.ClassAttendanceCheck
                .FirstOrDefaultAsync(c => c.CM_Id == cmId && c.Date == dateOnly);
            if (existingCheck == null)
            {
                return NotFound("ไม่พบข้อมูลการเช็คชื่อในวันที่ที่เลือก");
            }

            // ดึงข้อมูลการเช็คชื่อและนักเรียนในคลาส
            var existingAttendance = await _db.ClassAttendance
                .Where(a => a.CM_Id == cmId && a.Date == dateOnly)
                .ToListAsync();

            var studentsInClass = await _db.Students
                .Include(s => s.Profile)
                .Where(s => s.ClassId == classManagement.ClassId)
                .ToListAsync();

            var studentAttendanceList = studentsInClass.Select(s => new StudentAttendanceInputModel
            {
                StudentId = s.StudentId,
                StudentName = $"{s.Profile.FirstName} {s.Profile.LastName}",
                Status = "absent"
            }).ToList();

            foreach (var attendance in existingAttendance)
            {
                var student = studentAttendanceList.FirstOrDefault(s => s.StudentId == attendance.StudentId);
                if (student != null)
                {
                    student.Status = attendance.Status;
                }
            }

            var model = new AttendanceViewModel
            {
                cmId = cmId,
                date = date,
                ClassName = $"{classManagement.Class?.GradeLevels?.Name}/{classManagement.Class?.ClassNumber}",
                CourseName = classManagement.Course?.CourseName,
                Students = studentAttendanceList
            };
            _logger.LogWarning("sent {date}", date);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAttendance(AttendanceViewModel model)
        {
            Console.WriteLine($"Received JSON: {System.Text.Json.JsonSerializer.Serialize(model)}");
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState ไม่ถูกต้อง: {@ModelState}", ModelState);
                TempData["ErrorMessage"] = "Model validation failed. Please correct the errors and try again.";
                return View(model);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Unauthorized access. Please log in to access this information.";
                return Unauthorized("กรุณาเข้าสู่ระบบ");
            }

            using (var transaction = await _db.Database.BeginTransactionAsync())
            {
                try
                {
                    var attendanceDate = model.date;
                    var cmId = model.cmId;

                    var existingAttendanceCheck = await _db.ClassAttendanceCheck
                        .FirstOrDefaultAsync(c => c.CM_Id == cmId && c.Date == attendanceDate);

                    if (existingAttendanceCheck == null)
                    {
                        _logger.LogWarning("No CM_Id: {cmId}, Date: {attendanceDate}", cmId, attendanceDate);
                        TempData["ErrorMessage"] = "Attendance record not found for the specified date.";
                        return RedirectToAction("TeacherCourses", "Attendance");
                    }

                    // บันทึกใหม่
                    foreach (var student in model.Students)
                    {
                        var existingAttendanceRecord = await _db.ClassAttendance
                            .FirstOrDefaultAsync(a => a.CM_Id == cmId && a.StudentId == student.StudentId && a.Date == attendanceDate);

                        if (existingAttendanceRecord != null)
                        {
                            existingAttendanceRecord.Status = student.Status;
                        }
                        else
                        {
                            var attendanceRecord = new ClassAttendance
                            {
                                CM_Id = cmId,
                                StudentId = student.StudentId,
                                Status = student.Status,
                                Date = attendanceDate
                            };
                            _db.ClassAttendance.Add(attendanceRecord);
                        }

                        var summary = await _db.ClassAttendanceSummary
                            .FirstOrDefaultAsync(s => s.CM_Id == cmId && s.StudentId == student.StudentId);

                        if (summary == null)
                        {
                            summary = new ClassAttendanceSummary
                            {
                                CM_Id = cmId,
                                StudentId = student.StudentId,
                                PresentCount = 0,
                                AbsentCount = 0,
                                LateCount = 0,
                                ExcusedCount = 0,
                                UpdateAt = DateTime.UtcNow
                            };
                            _db.ClassAttendanceSummary.Add(summary);
                        }
                    }
                    await _db.SaveChangesAsync();

                    foreach (var student in model.Students)
                    {
                        var summary = await _db.ClassAttendanceSummary
                            .FirstOrDefaultAsync(s => s.CM_Id == cmId && s.StudentId == student.StudentId);

                        if (summary == null)
                        {
                            summary = new ClassAttendanceSummary
                            {
                                CM_Id = cmId,
                                StudentId = student.StudentId,
                                PresentCount = 0,
                                AbsentCount = 0,
                                LateCount = 0,
                                ExcusedCount = 0,
                                UpdateAt = DateTime.UtcNow
                            };
                            _db.ClassAttendanceSummary.Add(summary);
                        }

                        summary.PresentCount = await _db.ClassAttendance
                            .CountAsync(a => a.CM_Id == cmId && a.StudentId == student.StudentId && a.Status.ToLower() == "present");

                        summary.AbsentCount = await _db.ClassAttendance
                            .CountAsync(a => a.CM_Id == cmId && a.StudentId == student.StudentId && a.Status.ToLower() == "absent");

                        summary.LateCount = await _db.ClassAttendance
                            .CountAsync(a => a.CM_Id == cmId && a.StudentId == student.StudentId && a.Status.ToLower() == "late");

                        summary.ExcusedCount = await _db.ClassAttendance
                            .CountAsync(a => a.CM_Id == cmId && a.StudentId == student.StudentId && a.Status.ToLower() == "excused");

                        summary.UpdateAt = DateTime.UtcNow;

                        _db.ClassAttendanceSummary.Update(summary);
                    }

                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = "Attendance updated successfully!";
                    return RedirectToAction("TeacherCourses", "ClassAttendance");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "เกิดข้อผิดพลาดระหว่างการแก้ไขการเช็คชื่อ");
                    TempData["ErrorMessage"] = "An error occurred while updating attendance. Please try again.";
                    return View(model);
                }
            }
        }


        [HttpGet]
        public async Task<IActionResult> TeacherCourses()
        {
            var teacherId = await GetLoggedInTeacherIdAsync();
            if (teacherId == null)
            {
                return Unauthorized("กรุณาเข้าสู่ระบบเพื่อเข้าถึงข้อมูล");
            }

            // ตรวจสอบเทอมปัจจุบัน
            var currentSemester = await _db.Semesters
                .Where(s => DateTime.Now >= s.StartTime && DateTime.Now <= s.EndTime)
                .FirstOrDefaultAsync();
            if (currentSemester == null)
            {
                _logger.LogWarning("Current semester not found.");
                return NotFound("ไม่พบเทอมปัจจุบันในระบบ");
            }

            // ดึงข้อมูล ClassManagement ที่ครูสอนในเทอมปัจจุบัน
            var teacherCourses = await _db.ClassManagements
                .Where(cs => cs.Status == "Active")
                .Include(cm => cm.Course)
                .Include(cm => cm.Class)
                .ThenInclude(c => c.GradeLevels)
                .Where(cm => cm.TeacherId == teacherId && cm.SemesterId == currentSemester.SemesterID)
                .Select(cm => new TeacherCourseViewModel
                {
                    CM_Id = cm.CM_Id,
                    CourseName = cm.Course.CourseName,
                    CourseCode = cm.Course.Course_Code,
                    ClassNumber = cm.Class.ClassNumber,
                    ClassName = cm.Class.GradeLevels.Name,
                    SemesterId = cm.SemesterId
                })
                .Distinct()
                .ToListAsync();

            return View(teacherCourses);
        }
        [HttpGet]
        public async Task<IActionResult> ViewAttendanceByCourse(int cmId)
        {
            try
            {
                var course = await _db.ClassManagements
                    .Where(cs => cs.Status == "Active")
                    .FirstOrDefaultAsync(c => c.CM_Id == cmId);
                if (course == null)
                {
                    _logger.LogWarning($"Course with CM_Id: {cmId} not found");
                    return NotFound("ไม่พบข้อมูลวิชาที่ระบุ");
                }

                // ดึงข้อมูลการเข้าเรียนของนักเรียนในวิชานั้นๆ
                var attendanceSummary = await _db.ClassAttendanceSummary
                    .Where(s => s.CM_Id == cmId)
                    .Include(s => s.Student)
                    .ThenInclude(s => s.Profile) // รวมข้อมูล Profile ของ Student
                    .ToListAsync();

                if (!attendanceSummary.Any())
                {
                    _logger.LogInformation($"No attendance records found for CM_Id: {cmId}");
                }

                // แปลงข้อมูลจาก ClassAttendanceSummary เป็น ViewModel
                var viewModel = attendanceSummary.Select(s => new AttendanceSummaryViewModel
                {
                    StudentCode = s.Student?.Student_Code ?? "ไม่พบรหัสนักเรียน",
                    Name = s.Student?.Profile?.FirstName+" "+ s.Student?.Profile?.LastName ?? "ไม่พบชื่อ",
                    PresentCount = s.PresentCount ?? 0,
                    AbsentCount = s.AbsentCount ?? 0,
                    LateCount = s.LateCount ?? 0,
                    ExcusedCount = s.ExcusedCount ?? 0,
                }).ToList();

                @ViewBag.CM_Id = cmId;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving attendance for CM_Id: {cmId}");
                return StatusCode(500, "เกิดข้อผิดพลาดในการดึงข้อมูลการเข้าเรียน");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAttendanceSummary(int cmId)
        {
            try
            {
                var course = await _db.ClassManagements
                    .Where(cs => cs.Status == "Active")
                    .FirstOrDefaultAsync(c => c.CM_Id == cmId);
                if (course == null)
                {
                    _logger.LogWarning($"Course with CM_Id: {cmId} not found");
                    return NotFound("ไม่พบข้อมูลวิชาที่ระบุ");
                }

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("Unauthorized access attempt with empty user ID");
                    return Unauthorized("กรุณาเข้าสู่ระบบเพื่อเข้าถึงข้อมูล");
                }

                using (var transaction = await _db.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // ดึงข้อมูลทั้งหมดของนักเรียนในวิชานี้ที่มีการเข้าเรียน
                        var studentIds = await _db.ClassAttendance
                            .Where(a => a.CM_Id == cmId)
                            .Select(a => a.StudentId)
                            .Distinct()
                            .ToListAsync();

                        foreach (var studentId in studentIds)
                        {
                            // ค้นหาหรือสร้าง ClassAttendanceSummary
                            var summary = await _db.ClassAttendanceSummary
                                .FirstOrDefaultAsync(s => s.CM_Id == cmId && s.StudentId == studentId);

                            if (summary == null)
                            {
                                return RedirectToAction("ViewAttendanceByCourse", new { cmId });
                            }

                            // ดึงข้อมูลการเข้าเรียนแต่ละสถานะเพียงครั้งเดียว
                            var statuses = await _db.ClassAttendance
                                .Where(a => a.CM_Id == cmId && a.StudentId == studentId)
                                .GroupBy(a => a.Status.ToLower())
                                .Select(g => new { Status = g.Key, Count = g.Count() })
                                .ToListAsync();

                            // อัปเดตข้อมูลการเข้าเรียน
                            summary.PresentCount = statuses.FirstOrDefault(s => s.Status == "present")?.Count ?? 0;
                            summary.AbsentCount = statuses.FirstOrDefault(s => s.Status == "absent")?.Count ?? 0;
                            summary.LateCount = statuses.FirstOrDefault(s => s.Status == "late")?.Count ?? 0;
                            summary.ExcusedCount = statuses.FirstOrDefault(s => s.Status == "excused")?.Count ?? 0;
                            summary.UpdateAt = DateTime.UtcNow;

                            _db.ClassAttendanceSummary.Update(summary);
                        }

                        await _db.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation($"Attendance summary updated successfully for CM_Id: {cmId}");
                        return RedirectToAction("ViewAttendanceByCourse", new { cmId });
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        _logger.LogError(ex, $"Error updating attendance summary for CM_Id: {cmId}");
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating attendance summary for CM_Id: {cmId}");
                return StatusCode(500, "เกิดข้อผิดพลาดในการอัปเดตข้อมูลการเข้าเรียน");
            }
        }

    }
}
