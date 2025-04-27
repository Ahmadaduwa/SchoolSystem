using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Helpers;
using SchoolSystem.Models.ClassManagement;
using SchoolSystem.Models.CourseManagement;
using SchoolSystem.Models.RegistrationManagement;
using SchoolSystem.Models.UserManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SchoolSystem.Controllers
{
    public class ClassRegistrationController : Controller
    {
        private readonly AppDbContext _context;

        public ClassRegistrationController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("ClassRegistration/")]
        [Authorize(Policy = "AcademicPolicyOrAdminPolicy")]
        public async Task<IActionResult> IndexStudent(int? pageNumber, string searchString, string sortOrder, int? classFilter)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["CodeSortParam"] = sortOrder == "code" ? "code_desc" : "code";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentClass"] = classFilter; // เก็บค่าสำหรับการกรองห้อง

            // ดึงข้อมูลห้องโดย join ตาราง Class กับ GradeLevels เพื่อใช้ใน Dropdown
            var classList = await _context.Classes
                .Where(g => g.Status == "Active")
                .Include(c => c.GradeLevels)
                .Select(c => new SelectListItem
                {
                    Value = c.ClassId.ToString(),
                    Text = $"{c.GradeLevels.Name}/{c.ClassNumber}" // ตัวอย่าง "ป.1/1"
                })
                .ToListAsync();
            // เพิ่มตัวเลือก "All Classes" ไว้ที่หัวรายการ
            classList.Insert(0, new SelectListItem { Value = "", Text = "All Classes" });
            ViewData["Classes"] = classList;

            int pageSize = 10;
            var studentsQuery = _context.Students
                .Where(g => g.Status == "Active")
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
                _ => studentsQuery.OrderBy(s => s.Profile.LastName).ThenBy(s => s.Profile.FirstName),
            };

            int totalItems = await studentsQuery.CountAsync();
            ViewData["TotalItems"] = totalItems;

            return View(await PaginatedList<Student>.CreateAsync(studentsQuery, pageNumber ?? 1, pageSize));
        }

        // GET: api/Courses/ByCurriculum
        [HttpGet]
        [Route("api/Courses/ByCurriculum")]
        [Authorize(Policy = "AcademicPolicyOrAdminPolicy")]
        public async Task<IActionResult> GetCoursesByCurriculum(int curriculumId)
        {
            try
            {
                // ดึงรายการ CourseId จากแต่ละตารางที่เกี่ยวข้องกับหลักสูตร
                var compulsoryCourseIds = await _context.CompulsoryCourses
                    .Where(cc => cc.CurriculumId == curriculumId)
                    .Select(cc => cc.CourseId)
                    .ToListAsync();

                var compulsoryElectiveCourseIds = await _context.CompulsoryElectiveCourses
                    .Where(cec => cec.CurriculumId == curriculumId)
                    .Select(cec => cec.CourseId)
                    .ToListAsync();

                var electiveCourseIds = await _context.ElectiveCourses
                    .Where(ec => ec.CurriculumId == curriculumId)
                    .Select(ec => ec.CourseId)
                    .ToListAsync();

                var allCourseIds = compulsoryCourseIds
                    .Concat(compulsoryElectiveCourseIds)
                    .Concat(electiveCourseIds)
                    .Distinct()
                    .ToList();

                return Json(allCourseIds);
            }
            catch (Exception ex)
            {
                // log exception ตามความเหมาะสม
                return StatusCode(500, "เกิดข้อผิดพลาดในการดึงข้อมูลวิชา");
            }
        }

        // GET: ClassRegistration/Class
        [HttpGet]
        [Route("ClassRegistration/Class")]
        [Authorize(Policy = "AcademicPolicyOrAdminPolicy")]
        public async Task<IActionResult> Register()
        {
            try
            {
                var classes = await _context.Classes
                    .Where(c => c.Status == "Active")
                    .Include(c => c.GradeLevels)
                    .OrderBy(c => c.ClassNumber)
                    .ToListAsync();

                var courses = await _context.Course
                    .Where(c => c.Status == "Active")
                    .OrderBy(c => c.CourseName)
                    .ToListAsync();

                var semesters = await _context.Semesters
                    .Where(s => s.Status == "Active")
                    .OrderByDescending(s => s.SemesterYear)
                    .ThenByDescending(s => s.SemesterNumber)
                    .ToListAsync();

                var curriculums = await _context.Curriculum
                    .Where(s => s.Status == "Active")
                    .OrderBy(c => c.CurriculumName)
                    .ToListAsync();

                ViewBag.Classes = classes;
                ViewBag.Courses = courses;
                ViewBag.Semesters = semesters;
                ViewBag.Curriculums = curriculums;

                return View();
            }
            catch (Exception ex)
            {
                // บันทึก log ของ exception ได้ตามที่ต้องการ
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการโหลดข้อมูล กรุณาลองอีกครั้ง";
                return View();
            }
        }

        // ดำเนินการลงทะเบียน
        [HttpPost]
        [Route("ClassRegistration/Class")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AcademicPolicyOrAdminPolicy")]
        public async Task<IActionResult> Register(int classId, List<int> courseIds, int semesterId, int? curriculumId)
        {
            if (courseIds == null || !courseIds.Any())
            {
                TempData["ErrorMessage"] = "กรุณาเลือกอย่างน้อย 1 วิชา";
                return RedirectToAction("Register");
            }

            try
            {
                // ตรวจสอบว่ามีนักเรียนในห้องหรือไม่
                var students = await _context.Students
                    .Where(s => s.ClassId == classId)
                    .Select(s => s.StudentId)
                    .ToListAsync();

                if (!students.Any())
                {
                    TempData["ErrorMessage"] = "ไม่มีนักเรียนในห้องนี้";
                    return RedirectToAction("Register");
                }

                // ตรวจสอบว่านักเรียนมีการลงทะเบียนวิชาที่เลือกในภาคเรียนที่ระบุหรือไม่
                var existingRegistrations = await _context.RegisteredCourses
                    .Where(r => students.Contains(r.StudentId) &&
                                courseIds.Contains(r.CourseId) &&
                                r.SemesterId == semesterId)
                    .ToListAsync();

                if (existingRegistrations.Any())
                {
                    // แยกนักเรียนที่ลงทะเบียนแล้วออกมา
                    var duplicates = existingRegistrations
                        .GroupBy(r => r.StudentId)
                        .ToDictionary(g => g.Key, g => g.Select(r => r.CourseId).ToList());

                    int registrationCount = 0;
                    foreach (var studentId in students)
                    {
                        foreach (var courseId in courseIds)
                        {
                            if (!duplicates.ContainsKey(studentId) || !duplicates[studentId].Contains(courseId))
                            {
                                _context.RegisteredCourses.Add(new RegisteredCourse
                                {
                                    StudentId = studentId,
                                    CourseId = courseId,
                                    SemesterId = semesterId,
                                    Score = 0,
                                    Grade = 0,
                                    RegisteredDate = DateTime.Now
                                });
                                registrationCount++;
                            }
                        }
                    }

                    if (registrationCount > 0)
                    {
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = $"ลงทะเบียนสำเร็จ {registrationCount} รายการ (ข้ามรายการที่ซ้ำ)";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "นักเรียนทุกคนได้ลงทะเบียนในวิชาที่เลือกไว้แล้ว";
                    }
                }
                else
                {
                    // ลงทะเบียนวิชาทั้งหมดให้กับนักเรียนในห้อง
                    var registrations = new List<RegisteredCourse>();
                    foreach (var studentId in students)
                    {
                        foreach (var courseId in courseIds)
                        {
                            registrations.Add(new RegisteredCourse
                            {
                                StudentId = studentId,
                                CourseId = courseId,
                                SemesterId = semesterId,
                                Score = 0,
                                Grade = 0,
                                RegisteredDate = DateTime.Now
                            });
                        }
                    }

                    await _context.RegisteredCourses.AddRangeAsync(registrations);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"ลงทะเบียนสำเร็จ {registrations.Count} รายการ";
                }
            }
            catch (Exception ex)
            {
                // บันทึก log ของ exception ตามต้องการ
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการลงทะเบียน กรุณาลองอีกครั้ง";
            }

            return RedirectToAction("IndexStudent");
        }

        // GET: ClassRegistration/RegisterPerOne?studentId=123
        [HttpGet]
        [Route("ClassRegistration/std")]
        [Authorize(Policy = "AcademicPolicyOrAdminPolicy")]
        public async Task<IActionResult> RegisterPerOne(int id)
        {
            var student = await _context.Students
                .Where(g => g.Status == "Active")
                .Include(s => s.Profile)
                .FirstOrDefaultAsync(s => s.StudentId == id);

            if (student == null)
            {
                TempData["ErrorMessage"] = "ไม่พบข้อมูลนักเรียน";
                return RedirectToAction("IndexStudent");
            }

            // โหลดข้อมูลวิชา ภาคเรียน และหลักสูตร (ถ้าต้องการกรองวิชา)
            var courses = await _context.Course
                .Where(c => c.Status == "Active")
                .OrderBy(c => c.CourseName)
                .ToListAsync();

            var semesters = await _context.Semesters
                .Where(s => s.Status == "Active")
                .OrderByDescending(s => s.SemesterYear)
                .ThenByDescending(s => s.SemesterNumber)
                .ToListAsync();

            var curriculums = await _context.Curriculum
                .Where(c => c.Status == "Active")
                .OrderBy(c => c.CurriculumName)
                .ToListAsync();

            // ส่งข้อมูลไปยัง View ผ่าน ViewBag
            ViewBag.Student = student;
            ViewBag.Courses = courses;
            ViewBag.Semesters = semesters;
            ViewBag.Curriculums = curriculums;

            return View();
        }

        // POST: ClassRegistration/RegisterPerOne
        [HttpPost]
        [Route("ClassRegistration/std")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AcademicPolicyOrAdminPolicy")]
        public async Task<IActionResult> RegisterPerOne(int id, List<int> courseIds, int semesterId, int? curriculumId)
        {
            if (courseIds == null || !courseIds.Any())
            {
                TempData["ErrorMessage"] = "กรุณาเลือกอย่างน้อย 1 วิชา";
                return RedirectToAction("RegisterPerOne", new { id });
            }

            // ตรวจสอบว่ามีนักเรียนจริงหรือไม่
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                TempData["ErrorMessage"] = "ไม่พบข้อมูลนักเรียน";
                return RedirectToAction("IndexStudent");
            }

            try
            {
                // ตรวจสอบว่ามีการลงทะเบียนวิชาในภาคเรียนเดียวกันหรือไม่
                var existingRegistrations = await _context.RegisteredCourses
                    .Where(r => r.StudentId == id &&
                                courseIds.Contains(r.CourseId) &&
                                r.SemesterId == semesterId)
                    .ToListAsync();

                // เก็บรายวิชาที่นักเรียนได้ลงทะเบียนแล้ว
                var duplicateCourseIds = existingRegistrations.Select(r => r.CourseId).ToList();
                int registrationCount = 0;

                foreach (var courseId in courseIds)
                {
                    if (!duplicateCourseIds.Contains(courseId))
                    {
                        _context.RegisteredCourses.Add(new RegisteredCourse
                        {
                            StudentId = id,
                            CourseId = courseId,
                            SemesterId = semesterId,
                            Score = 0,
                            Grade = 0,
                            RegisteredDate = DateTime.Now
                        });
                        registrationCount++;
                    }
                }

                if (registrationCount > 0)
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"ลงทะเบียนสำเร็จ {registrationCount} รายการ (ข้ามรายการที่ซ้ำ)";
                }
                else
                {
                    TempData["ErrorMessage"] = "นักเรียนได้ลงทะเบียนวิชาที่เลือกในภาคเรียนนี้แล้ว";
                }
            }
            catch (Exception ex)
            {
                // สามารถบันทึก log ของ exception ได้ตามต้องการ
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการลงทะเบียน กรุณาลองอีกครั้ง";
            }

            return RedirectToAction("IndexStudent");
        }

        [HttpGet]
        [Route("ClassRegistration/ViewRegistration")]
        [Authorize(Policy = "AcademicPolicyOrAdminPolicyOrStudentPolicy")]
        public async Task<IActionResult> ViewRegistration(int id, int? semesterId)
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
            var isStudent = User.IsInRole("Student");

            if (isStudent)
            {

                var profile = await _context.Profiles
                    .Include(p => p.Student) 
                    .FirstOrDefaultAsync(p => p.UserId == userId);

                if (profile == null || profile.Student == null || profile.Student.StudentId != id)
                {
                    TempData["ErrorMessage"] = "คุณไม่มีสิทธิ์ดูข้อมูลการลงทะเบียนของนักเรียนคนอื่น";
                    return RedirectToAction("Home", "Home");
                }
            }

            var registeredSemesterIds = await _context.RegisteredCourses
                .Where(r => r.StudentId == id)
                .Select(r => r.SemesterId)
                .Distinct()
                .ToListAsync();

            if (!registeredSemesterIds.Any())
            {
                TempData["ErrorMessage"] = "ไม่พบการลงทะเบียนของนักเรียน";
                return RedirectToAction("IndexStudent");
            }

            var semesters = await _context.Semesters
                .Where(s => registeredSemesterIds.Contains(s.SemesterID))
                .OrderByDescending(s => s.SemesterYear)
                .ThenByDescending(s => s.SemesterNumber)
                .ToListAsync();

            int selectedSemesterId = semesterId.HasValue && registeredSemesterIds.Contains(semesterId.Value)
                ? semesterId.Value
                : semesters.First().SemesterID;

            semesters = semesters.OrderBy(s => s.SemesterID == selectedSemesterId ? 0 : 1)
                        .ThenByDescending(s => s.SemesterYear)
                        .ThenByDescending(s => s.SemesterNumber)
                        .ToList();

            // ดึงการลงทะเบียนของนักเรียนในภาคเรียนที่เลือก พร้อมข้อมูลวิชา
            var registrations = await _context.RegisteredCourses
                .Where(r => r.StudentId == id && r.SemesterId == selectedSemesterId)
                .Include(r => r.Course)
                .ToListAsync();

            // คำนวณ GPA
            float totalGradePoints = 0;
            float totalCredits = 0;
            float gpa = 0;

            if (registrations.Any())
            {
                foreach (var reg in registrations)
                {
                    totalGradePoints += reg.Grade * reg.Course.Unit;
                    totalCredits += reg.Course.Unit;
                }

                gpa = totalCredits > 0 ? totalGradePoints / totalCredits : 0;
            }

            // ดึงข้อมูลนักเรียน (ถ้าต้องการแสดงชื่อ)
            var studentData = await _context.Students
                .Where(s => s.Status == "Active")
                .Include(s => s.Profile)
                .FirstOrDefaultAsync(s => s.StudentId == id);

            if (studentData == null)
            {
                TempData["ErrorMessage"] = "ไม่พบข้อมูลนักเรียน";
                return RedirectToAction("IndexStudent");
            }

            // ส่งข้อมูลไปยัง View ผ่าน ViewBag
            ViewBag.Student = studentData;
            ViewBag.Semesters = semesters;
            ViewBag.SelectedSemesterId = selectedSemesterId;
            ViewBag.Registrations = registrations;
            ViewBag.GPA = gpa;
            ViewBag.TotalCredits = totalCredits;

            return View();
        }

        // POST: ClassRegistration/DeleteRegistration
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("ClassRegistration/DeleteRegistration")]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> DeleteRegistration(int registrationId, int studentId, int selectedSemesterId)
        {
            try
            {
                var registration = await _context.RegisteredCourses.FindAsync(registrationId);
                if (registration == null)
                {
                    TempData["ErrorMessage"] = "ไม่พบการลงทะเบียนที่ต้องการลบ";
                    return RedirectToAction("ViewRegistration", new { id = studentId, semesterId = selectedSemesterId });
                }

                _context.RegisteredCourses.Remove(registration);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "ยกเลิกการลงทะเบียนเรียบร้อย";
            }
            catch (Exception ex)
            {
                // บันทึก log ได้ตามต้องการ
                TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการยกเลิกการลงทะเบียน";
            }

            return RedirectToAction("ViewRegistration", new { id = studentId, semesterId = selectedSemesterId });
        }

        [HttpGet]
        [Authorize(Policy = "StudentPolicy")]
        public async Task<IActionResult> stdGet()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isStudent = User.IsInRole("Student");

            var profile = await _context.Profiles
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            return RedirectToAction("ViewRegistration", new { id = profile.Student.StudentId });
        }

    }
}