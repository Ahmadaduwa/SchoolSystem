using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models.ClassManagement;
using SchoolSystem.Models.Assignment;
using SchoolSystem.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using SchoolSystem.Models.RegistrationManagement;
using System.Transactions;
using SchoolSystem.ViewModels;
using System.Security.Claims;

namespace SchoolSystem.Controllers
{
    [Authorize(Policy = "TeacherPolicy")]
    public class AssignmentController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<ClassAttendanceController> _logger;

        public AssignmentController(AppDbContext db, ILogger<ClassAttendanceController> logger)
        {
            _db = db;
            _logger = logger;
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
                .ThenInclude(c=> c.GradeLevels)
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

        public async Task<IActionResult> Index(int cmId)
        {
            var course = await GetClassManagementWithCourseAsync(cmId);
            ViewBag.Course = course?.Course?.CourseName ?? "Unknown";
            ViewBag.CM_Id = cmId;

            var assignments = await _db.Assignments
                .Where(a => a.CM_Id == cmId)
                .Include(a => a.AssignmentScores)
                .ToListAsync();

            return View(assignments);
        }

        // GET: Assignments/Create?cmId=123
        public async Task<IActionResult> Create(int cmId)
        {
            var course = await GetClassManagementWithCourseAsync(cmId);
            ViewBag.Course = course?.Course?.CourseName ?? "Unknown";
            ViewBag.CM_Id = cmId;
            ViewBag.RemainingRealScore = await CalculateRemainingRealScoreAsync(cmId);

            var assignment = new Assignment
            {
                AssignedDate = DateTime.UtcNow,
                CM_Id = cmId
            };

            return View(assignment);
        }

        /// <summary>
        /// Handles the creation of a new assignment.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Assignment assignment)
        {
            if (!ModelState.IsValid)
            {
                await PrepareCreateViewBagAsync(assignment.CM_Id);
                return View(assignment);
            }

            if (assignment.DueDate < assignment.AssignedDate)
            {
                TempData["ErrorMessage"] = "วันครบกำหนดต้องไม่ต่ำกว่าวันที่มอบหมาย";
                await PrepareCreateViewBagAsync(assignment.CM_Id);
                return View(assignment);
            }

            var currentTotalRealScore = await _db.Assignments
                .Where(a => a.CM_Id == assignment.CM_Id)
                .SumAsync(a => (float?)a.RealScore) ?? 0;

            if (currentTotalRealScore + assignment.RealScore > 100)
            {
                TempData["ErrorMessage"] = "รวมคะแนนจริงของงานทั้งหมดไม่สามารถเกิน 100 ได้";
                await PrepareCreateViewBagAsync(assignment.CM_Id);
                return View(assignment);
            }

            try
            {
                _db.Assignments.Add(assignment);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "เพิ่มงานเรียบร้อยแล้ว";
                return RedirectToAction("Index", new { cmId = assignment.CM_Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating assignment for CM_Id {CM_Id}", assignment.CM_Id);
                ModelState.AddModelError("", "เกิดข้อผิดพลาดในการบันทึกข้อมูล");
                await PrepareCreateViewBagAsync(assignment.CM_Id);
                return View(assignment);
            }
        }  

        public async Task<IActionResult> Details(int id, int cmId)
        {
            var course = await GetClassManagementWithCourseAsync(cmId);
            ViewBag.Course = course?.Course?.CourseName ?? "Unknown";
            ViewBag.CM_Id = cmId;

            var assignment = await _db.Assignments
                .Include(a => a.AssignmentScores)
                .ThenInclude(ascore => ascore.Student)
                .ThenInclude(s => s.Profile)
                .FirstOrDefaultAsync(a => a.AssignmentId == id && a.CM_Id == cmId);

            if (assignment == null)
            {
                return NotFound();
            }

            assignment.AssignmentScores = assignment.AssignmentScores
                .Where(ascore => ascore.Student != null && ascore.Student.Status == "Active")
                .ToList();

            return View(assignment);
        }


        public async Task<IActionResult> Edit(int id, int cmId)
        {
            var assignment = await _db.Assignments
                .FirstOrDefaultAsync(a => a.AssignmentId == id && a.CM_Id == cmId);

            if (assignment == null)
            {
                return NotFound();
            }

            var course = await GetClassManagementWithCourseAsync(cmId);
            ViewBag.Course = course?.Course?.CourseName ?? "Unknown";
            ViewBag.CM_Id = cmId;
            ViewBag.RemainingRealScore = await CalculateRemainingRealScoreAsync(cmId, id);

            return View(assignment);
        }

        /// <summary>
        /// Handles the update of an existing assignment.
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Assignment assignment)
        {
            if (id != assignment.AssignmentId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                await PrepareEditViewBagAsync(assignment.CM_Id, id);
                return View(assignment);
            }

            if (assignment.DueDate < assignment.AssignedDate)
            {
                TempData["ErrorMessage"] = "วันครบกำหนดต้องไม่ต่ำกว่าวันที่มอบหมาย";
                await PrepareEditViewBagAsync(assignment.CM_Id, id);
                return View(assignment);
            }

            var totalRealScoreOthers = await _db.Assignments
                .Where(a => a.CM_Id == assignment.CM_Id && a.AssignmentId != assignment.AssignmentId)
                .SumAsync(a => (float?)a.RealScore) ?? 0;

            if (totalRealScoreOthers + assignment.RealScore > 100)
            {
                TempData["ErrorMessage"] = "รวมคะแนนจริงของงานทั้งหมดไม่สามารถเกิน 100 ได้";
                await PrepareEditViewBagAsync(assignment.CM_Id, id);
                return View(assignment);
            }

            try
            {
                _db.Update(assignment);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "แก้ไขงานเรียบร้อยแล้ว";
                return RedirectToAction("Index", new { cmId = assignment.CM_Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating assignment {AssignmentId} for CM_Id {CM_Id}", id, assignment.CM_Id);
                ModelState.AddModelError("", "เกิดข้อผิดพลาดในการอัพเดทข้อมูล");
                await PrepareEditViewBagAsync(assignment.CM_Id, id);
                return View(assignment);
            }
        }
        public async Task<IActionResult> Grade(int id, int cmId)
        {
            var assignment = await _db.Assignments
                .FirstOrDefaultAsync(a => a.AssignmentId == id && a.CM_Id == cmId);

            if (assignment == null)
            {
                return NotFound();
            }

            var classMgmt = await _db.ClassManagements
                .FirstOrDefaultAsync(cm => cm.CM_Id == cmId);

            if (classMgmt == null)
            {
                return NotFound();
            }

            var students = await _db.Students
                .Include(s => s.Profile)
                .Where(s => s.ClassId == classMgmt.ClassId && s.Status == "Active")
                .ToListAsync();

            var viewModel = new AssignmentGradingViewModel
            {
                AssignmentId = assignment.AssignmentId,
                AssignmentTitle = assignment.Title,
                FullScore = assignment.FullScore,
                DueDate = assignment.DueDate,
                CM_Id = cmId,
                StudentScores = students.Select(s => new StudentScoreViewModel
                {
                    StudentId = s.StudentId,
                    Student_Code = s.Student_Code ?? "N/A",
                    StudentName = s.Profile != null
                        ? $"{s.Profile.FirstName} {s.Profile.LastName}"
                        : "ไม่ระบุชื่อ",
                    Score = _db.AssignmentScores
                        .Where(ascore => ascore.AssignmentId == assignment.AssignmentId && ascore.StudentId == s.StudentId)
                        .Select(ascore => ascore.Score)
                        .FirstOrDefault()
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Grade(AssignmentGradingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                foreach (var studentScore in model.StudentScores)
                {

                    var existingScore = await _db.AssignmentScores
                        .FirstOrDefaultAsync(ascore => ascore.AssignmentId == model.AssignmentId && ascore.StudentId == studentScore.StudentId);

                    if (existingScore != null)
                    {
                        existingScore.Score = studentScore.Score;
                        existingScore.SubmittedDate = DateTime.UtcNow;
                        _db.AssignmentScores.Update(existingScore);
                    }
                    else
                    {
                        var newScore = new AssignmentScore
                        {
                            AssignmentId = model.AssignmentId,
                            StudentId = studentScore.StudentId,
                            Score = studentScore.Score,
                            SubmittedDate = DateTime.UtcNow
                        };
                        _db.AssignmentScores.Add(newScore);
                    }
                }

                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "บันทึกคะแนนเรียบร้อยแล้ว";
                return RedirectToAction("Details", new { id = model.AssignmentId, cmId = model.CM_Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error grading assignment {AssignmentId} for CM_Id {CM_Id}", model.AssignmentId, model.CM_Id);
                ModelState.AddModelError("", "เกิดข้อผิดพลาดในการบันทึกคะแนน");
                return View(model);
            }
        }

        public async Task<IActionResult> Summary(int cmId)
        {
            var classManagement = await GetClassManagementWithCourseAsync(cmId, onlyActive: true);
            if (classManagement == null)
            {
                _logger.LogWarning("ClassManagement not found for CM_Id {CM_Id}", cmId);
                return NotFound("ClassManagement not found.");
            }

            var students = await _db.Students
                .Include(s => s.Profile)
                .Where(s => s.ClassId == classManagement.ClassId && s.Status == "Active")
                .ToListAsync();

            var assignments = await _db.Assignments
                .Include(a => a.AssignmentScores)
                .Where(a => a.CM_Id == cmId)
                .ToListAsync();

            var viewModel = new AssignmentSummaryViewModel
            {
                CM_Id = cmId,
                CourseName = classManagement.Course?.CourseName ?? "Unknown",
                StudentSummaries = students.Select(s =>
                {
                    float totalScore = CalculateStudentTotalScore(s.StudentId, assignments);

                    // Validate totalScore before calculating grade
                    if (totalScore < 0 || float.IsNaN(totalScore) || float.IsInfinity(totalScore))
                    {
                        _logger.LogWarning("Invalid TotalScore {TotalScore} for StudentId {StudentId}", totalScore, s.StudentId);
                        totalScore = 0;
                    }

                    var isRegistered = _db.RegisteredCourses
                        .Any(rc => rc.StudentId == s.StudentId &&
                                   rc.CourseId == classManagement.CourseId &&
                                   rc.SemesterId == classManagement.SemesterId);

                    return new StudentSummaryViewModel
                    {
                        StudentId = s.StudentId,
                        StudentCode = s.Student_Code ?? "N/A",
                        StudentName = s.Profile != null
                            ? $"{s.Profile.FirstName} {s.Profile.LastName}"
                            : "Unknown",
                        TotalScore = Math.Clamp(totalScore, 0, 100), // Ensure score is within 0-100
                        Grade = CalculateGrade(totalScore),
                        IsRegistered = isRegistered
                    };
                }).ToList()
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Summary(AssignmentSummaryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "ข้อมูลไม่ถูกต้อง กรุณาตรวจสอบอีกครั้ง";
                return RedirectToAction("Summary", new { cmId = model.CM_Id });
            }

            var classManagement = await GetClassManagementWithCourseAsync(model.CM_Id);
            if (classManagement == null)
            {
                _logger.LogWarning("ClassManagement not found for CM_Id {CM_Id}", model.CM_Id);
                TempData["ErrorMessage"] = "ไม่พบข้อมูล ClassManagement";
                return RedirectToAction("Summary", new { cmId = model.CM_Id });
            }

            try
            {
                using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                foreach (var studentSummary in model.StudentSummaries)
                {
                    if (studentSummary.TotalScore < 0 || studentSummary.TotalScore > 100)
                    {
                        TempData["ErrorMessage"] = $"คะแนนของ {studentSummary.StudentName} ต้องอยู่ระหว่าง 0 และ 100";
                        return RedirectToAction("Summary", new { cmId = model.CM_Id });
                    }

                    var registeredCourse = await _db.RegisteredCourses
                        .FirstOrDefaultAsync(rc => rc.StudentId == studentSummary.StudentId &&
                                                   rc.CourseId == classManagement.CourseId &&
                                                   rc.SemesterId == classManagement.SemesterId);

                    if (registeredCourse != null)
                    {
                        registeredCourse.Score = studentSummary.TotalScore;
                        registeredCourse.Grade = CalculateGrade(studentSummary.TotalScore);
                        _db.RegisteredCourses.Update(registeredCourse);
                    }

                    // ======= เพิ่มส่วนคำนวณ GPAX ตรงนี้ =======
                    var registrations = await _db.RegisteredCourses
                        .Where(r => r.StudentId == studentSummary.StudentId)
                        .Include(r => r.Course)
                        .ToListAsync();

                    if (registrations.Any())
                    {
                        float totalGradePoints = 0;
                        float totalCredits = 0;

                        foreach (var reg in registrations)
                        {
                            totalGradePoints += reg.Grade * reg.Course.Unit;
                            totalCredits += reg.Course.Unit;
                        }

                        float gpax = totalCredits > 0 ? totalGradePoints / totalCredits : 0;

                        var student = await _db.Students.FirstOrDefaultAsync(s => s.StudentId == studentSummary.StudentId);
                        if (student != null)
                        {
                            student.GPA = gpax;
                            _db.Students.Update(student);
                        }
                    }
                }

                await _db.SaveChangesAsync();
                transaction.Complete();

                TempData["SuccessMessage"] = "อัพเดทคะแนนและ GPAX เรียบร้อยแล้ว";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating scores and GPAX for CM_Id {CM_Id}", model.CM_Id);
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการอัพเดทคะแนน กรุณาลองใหม่";
            }

            return RedirectToAction("Summary", new { cmId = model.CM_Id });
        }


        private async Task<ClassManagement?> GetClassManagementWithCourseAsync(int cmId, bool onlyActive = false)
        {
            var query = _db.ClassManagements
                .Include(cm => cm.Course)
                .Where(cm => cm.CM_Id == cmId);

            if (onlyActive)
            {
                query = query.Where(cm => cm.Status == "Active");
            }

            return await query.FirstOrDefaultAsync();
        }

        private async Task<float> CalculateRemainingRealScoreAsync(int cmId, int? excludeAssignmentId = null)
        {
            var totalRealScore = await _db.Assignments
                .Where(a => a.CM_Id == cmId && (excludeAssignmentId == null || a.AssignmentId != excludeAssignmentId))
                .SumAsync(a => (float?)a.RealScore) ?? 0;

            return 100 - totalRealScore;
        }

        private async Task PrepareCreateViewBagAsync(int cmId)
        {
            var course = await GetClassManagementWithCourseAsync(cmId);
            ViewBag.Course = course?.Course?.CourseName ?? "Unknown";
            ViewBag.CM_Id = cmId;
            ViewBag.RemainingRealScore = await CalculateRemainingRealScoreAsync(cmId);
        }

        private async Task PrepareEditViewBagAsync(int cmId, int assignmentId)
        {
            var course = await GetClassManagementWithCourseAsync(cmId);
            ViewBag.Course = course?.Course?.CourseName ?? "Unknown";
            ViewBag.CM_Id = cmId;
            ViewBag.RemainingRealScore = await CalculateRemainingRealScoreAsync(cmId, assignmentId);
        }

        private float CalculateStudentTotalScore(int studentId, List<Assignment> assignments)
        {
            float totalScore = 0;
            foreach (var assignment in assignments)
            {
                var score = assignment.AssignmentScores
                    .FirstOrDefault(asc => asc.StudentId == studentId)?.Score ?? 0;
                if (assignment.FullScore > 0)
                {
                    totalScore += (score / assignment.FullScore) * assignment.RealScore;
                }
            }
            return totalScore;
        }

        private float CalculateGrade(float score)
        {
            if (score >= 80) return 4.0f;
            if (score >= 75) return 3.5f;
            if (score >= 70) return 3.0f;
            if (score >= 65) return 2.5f;
            if (score >= 60) return 2.0f;
            if (score >= 55) return 1.5f;
            if (score >= 50) return 1.0f;
            return 0.0f;
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
    }
}
