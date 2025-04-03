using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Helpers;
using SchoolSystem.Models.ClassManagement;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSystem.Controllers
{
    [Authorize(Policy = "AcademicPolicyOrAdminPolicy")]
    public class ClassManagementController : Controller
    {
        private readonly AppDbContext _db;

        public ClassManagementController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Route("ClassManagement")]
        public async Task<IActionResult> Index(int? pageNumber, string searchString, string sortOrder, string statusFilter, int? classFilter, int? teacherFilter, int? courseFilter, int? semesterFilter)
        {
            // เก็บข้อมูลสำหรับ sorting และ filtering ลง ViewData
            ViewData["CurrentSort"] = sortOrder;
            ViewData["ClassSortParam"] = sortOrder == "class" ? "class_desc" : "class";
            ViewData["TeacherSortParam"] = sortOrder == "teacher" ? "teacher_desc" : "teacher";
            ViewData["CourseSortParam"] = sortOrder == "course" ? "course_desc" : "course";
            ViewData["SemesterSortParam"] = sortOrder == "semester" ? "semester_desc" : "semester";
            ViewData["StatusSortParam"] = sortOrder == "status" ? "status_desc" : "status";

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = statusFilter;
            ViewData["CurrentClass"] = classFilter;
            ViewData["CurrentTeacher"] = teacherFilter;
            ViewData["CurrentCourse"] = courseFilter;
            ViewData["CurrentSemester"] = semesterFilter;

            // สร้างรายการสถานะเพื่อใช้เป็นตัวกรอง
            var statuses = await _db.ClassManagements
                .Select(c => c.Status)
                .Distinct()
                .ToListAsync();
            ViewData["Statuses"] = statuses
                .Select(s => new SelectListItem { Value = s, Text = s })
                .ToList();

            // เตรียมข้อมูลตัวกรอง
            ViewData["Classes"] = await _db.Classes
                .Where(c => c.Status == "Active")
                .Include(c => c.GradeLevels)
                .Select(c => new SelectListItem
                {
                    Value = c.ClassId.ToString(),
                    Text = c.GradeLevels!.Name + "/" + c.ClassNumber,
                    Selected = classFilter.HasValue && c.ClassId == classFilter.Value
                }).ToListAsync();

            ViewData["Teachers"] = await _db.Teachers
                .Where(c => c.Status == "Active")
                .Include(t => t.Profile)
                .Select(t => new SelectListItem
                {
                    Value = t.TeacherId.ToString(),
                    Text = t.Profile.FirstName + " " + t.Profile.LastName,
                    Selected = teacherFilter.HasValue && t.TeacherId == teacherFilter.Value
                }).ToListAsync();

            ViewData["Courses"] = await _db.Course
                .Where(c => c.Status == "Active")
                .Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = c.CourseName,
                    Selected = courseFilter.HasValue && c.CourseId == courseFilter.Value
                }).ToListAsync();

            ViewData["Semesters"] = await _db.Semesters
                .Where(c => c.Status == "Active")
                .Select(s => new SelectListItem
                {
                    Value = s.SemesterID.ToString(),
                    Text = s.SemesterYear.ToString() + "/" + s.SemesterNumber.ToString(),
                    Selected = semesterFilter.HasValue && s.SemesterID == semesterFilter.Value
                }).ToListAsync();

            // สร้าง query เริ่มต้น
            var classManagementQuery = _db.ClassManagements
                .Include(c => c.Class)
                    .ThenInclude(c => c!.GradeLevels)
                .Include(c => c.Teacher)
                    .ThenInclude(t => t!.Profile)
                .Include(c => c.Course)
                .Include(c => c.Semester)
                .AsNoTracking();

            // ค้นหาตามคำ
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                classManagementQuery = classManagementQuery.Where(c =>
                    (c.Class != null && c.Class.GradeLevels != null && c.Class.GradeLevels.Name.Contains(searchString)) ||
                    (c.Teacher != null && c.Teacher.Profile != null &&
                     (c.Teacher.Profile.FirstName.Contains(searchString) || c.Teacher.Profile.LastName.Contains(searchString))) ||
                    (c.Course != null && c.Course.CourseName.Contains(searchString)) ||
                    (c.ScoringCriteria != null && c.ScoringCriteria.Contains(searchString)));
            }

            // กรองตามสถานะ
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                classManagementQuery = classManagementQuery.Where(c => c.Status == statusFilter);
            }

            // กรองตามคลาส
            if (classFilter.HasValue)
            {
                classManagementQuery = classManagementQuery.Where(c => c.ClassId == classFilter.Value);
            }

            // กรองตามครู
            if (teacherFilter.HasValue)
            {
                classManagementQuery = classManagementQuery.Where(c => c.TeacherId == teacherFilter.Value);
            }

            // กรองตามวิชา
            if (courseFilter.HasValue)
            {
                classManagementQuery = classManagementQuery.Where(c => c.CourseId == courseFilter.Value);
            }

            // กรองตามภาคเรียน
            if (semesterFilter.HasValue)
            {
                classManagementQuery = classManagementQuery.Where(c => c.SemesterId == semesterFilter.Value);
            }

            // จัดเรียงข้อมูล
            classManagementQuery = sortOrder switch
            {
                "class" => classManagementQuery.OrderBy(c => c.Class!.GradeLevels!.Name).ThenBy(c => c.Class!.ClassNumber),
                "class_desc" => classManagementQuery.OrderByDescending(c => c.Class!.GradeLevels!.Name).ThenByDescending(c => c.Class!.ClassNumber),
                "teacher" => classManagementQuery.OrderBy(c => c.Teacher!.Profile!.FirstName).ThenBy(c => c.Teacher!.Profile!.LastName),
                "teacher_desc" => classManagementQuery.OrderByDescending(c => c.Teacher!.Profile!.FirstName).ThenByDescending(c => c.Teacher!.Profile!.LastName),
                "course" => classManagementQuery.OrderBy(c => c.Course!.CourseName),
                "course_desc" => classManagementQuery.OrderByDescending(c => c.Course!.CourseName),
                "semester" => classManagementQuery.OrderBy(c => c.Semester!.SemesterYear).ThenBy(c => c.Semester!.SemesterNumber),
                "semester_desc" => classManagementQuery.OrderByDescending(c => c.Semester!.SemesterYear).ThenByDescending(c => c.Semester!.SemesterNumber),
                "status" => classManagementQuery.OrderBy(c => c.Status),
                "status_desc" => classManagementQuery.OrderByDescending(c => c.Status),
                _ => classManagementQuery.OrderBy(c => c.Class!.GradeLevels!.Name).ThenBy(c => c.Class!.ClassNumber),
            };

            // แบ่งหน้า
            int pageSize = 10;
            var totalItems = await classManagementQuery.CountAsync();
            ViewData["TotalItems"] = totalItems;
            var pagedClassManagement = await PaginatedList<ClassManagement>.CreateAsync(classManagementQuery, pageNumber ?? 1, pageSize);

            return View(pagedClassManagement);
        }


        // GET: ClassManagement/Create
        [HttpGet]
        [Route("ClassManagement/Create")]
        public async Task<IActionResult> CreateManage(int? classFilter, int? teacherFilter, int? courseFilter, int? semesterFilter)
        {
            // เตรียมข้อมูลตัวกรอง
            ViewData["Classes"] = await _db.Classes
                .Where(c => c.Status == "Active")
                .Include(c => c.GradeLevels)
                .Select(c => new SelectListItem
                {
                    Value = c.ClassId.ToString(),
                    Text = c.GradeLevels!.Name + " / " + c.ClassNumber,
                    Selected = classFilter.HasValue && c.ClassId == classFilter.Value
                }).ToListAsync();

            ViewData["Teachers"] = await _db.Teachers
                .Where(t => t.Status == "Active")
                .Include(t => t.Profile)
                .Select(t => new SelectListItem
                {
                    Value = t.TeacherId.ToString(),
                    Text = t.Profile.FirstName + " " + t.Profile.LastName,
                    Selected = teacherFilter.HasValue && t.TeacherId == teacherFilter.Value
                }).ToListAsync();

            ViewData["Courses"] = await _db.Course
                .Where(c => c.Status == "Active")
                .Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = c.CourseName,
                    Selected = courseFilter.HasValue && c.CourseId == courseFilter.Value
                }).ToListAsync();

            ViewData["Semesters"] = await _db.Semesters
                .Where(s => s.Status == "Active")
                .Select(s => new SelectListItem
                {
                    Value = s.SemesterID.ToString(),
                    Text = s.SemesterYear.ToString() + " / " + s.SemesterNumber.ToString(),
                    Selected = semesterFilter.HasValue && s.SemesterID == semesterFilter.Value
                }).ToListAsync();

            return View();
        }

        // POST: ClassManagement/Create
        [HttpPost]
        [Route("ClassManagement/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateManage(ClassManagement newClassManagement)
        {
            if (!ModelState.IsValid)
            {
                // กรณี validation ไม่ผ่าน ให้ re-populate ViewData ด้วย
                await PopulateDropdowns();
                return View(newClassManagement);
            }

            try
            {
                _db.ClassManagements.Add(newClassManagement);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Class management record created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Database error: {ex.Message}");
            }

            // หากเกิดข้อผิดพลาด re-populate ViewData อีกครั้ง
            await PopulateDropdowns();
            return View(newClassManagement);
        }

        private async Task PopulateDropdowns(ClassManagement cm = null)
        {
            ViewData["Classes"] = await _db.Classes
                .Where(c => c.Status == "Active")
                .Include(c => c.GradeLevels)
                .Select(c => new SelectListItem
                {
                    Value = c.ClassId.ToString(),
                    Text = c.GradeLevels!.Name + " / " + c.ClassNumber,
                    Selected = cm != null && c.ClassId == cm.ClassId
                }).ToListAsync();

            ViewData["Teachers"] = await _db.Teachers
                .Where(t => t.Status == "Active")
                .Include(t => t.Profile)
                .Select(t => new SelectListItem
                {
                    Value = t.TeacherId.ToString(),
                    Text = t.Profile.FirstName + " " + t.Profile.LastName,
                    Selected = cm != null && t.TeacherId == cm.TeacherId
                }).ToListAsync();

            ViewData["Courses"] = await _db.Course
                .Where(c => c.Status == "Active")
                .Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = c.CourseName,
                    Selected = cm != null && c.CourseId == cm.CourseId
                }).ToListAsync();

            ViewData["Semesters"] = await _db.Semesters
                .Where(s => s.Status == "Active")
                .Select(s => new SelectListItem
                {
                    Value = s.SemesterID.ToString(),
                    Text = s.SemesterYear.ToString() + " / " + s.SemesterNumber.ToString(),
                    Selected = cm != null && s.SemesterID == cm.SemesterId
                }).ToListAsync();
        }

        // GET: ClassManagement/Edit/{id}
        [HttpGet]
        [Route("ClassManagement/Edit/{id}")]
        public async Task<IActionResult> EditManage(int id)
        {
            var classManagement = await _db.ClassManagements.FindAsync(id);
            if (classManagement == null)
            {
                return NotFound();
            }
            await PopulateDropdowns(classManagement);
            return View(classManagement);
        }

        [HttpPost]
        [Route("ClassManagement/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditManage(ClassManagement updatedCM)
        {
            var existingCM = await _db.ClassManagements.FindAsync(updatedCM.CM_Id);
            if (existingCM == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await PopulateDropdowns(updatedCM);
                return View(updatedCM);
            }

            try
            {
                // รักษาค่า CreatedAt ไว้เหมือนเดิม และปรับปรุง UpdatedAt ให้เป็นเวลาปัจจุบัน
                updatedCM.CreateAt = existingCM.CreateAt;
                updatedCM.UpdateAt = DateTime.Now;

                // Detach entity เดิมออกเพื่อหลีกเลี่ยงปัญหา tracking
                _db.Entry(existingCM).State = EntityState.Detached;

                _db.ClassManagements.Update(updatedCM);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Class management updated successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating class management: {ex.Message}");
                await PopulateDropdowns(updatedCM);
                return View(updatedCM);
            }
        }

        

        [HttpGet]
        [Route("ClassManagement/Details/{id}")]
        public async Task<IActionResult> ViewManage(int id)
        {
            if (id == null)
                return NotFound();

            var classManagement = await _db.ClassManagements
                .Include(cm => cm.Class)
                    .ThenInclude(c => c.GradeLevels)
                .Include(cm => cm.Teacher)
                    .ThenInclude(t => t.Profile)
                .Include(cm => cm.Course)
                .Include(cm => cm.Semester)
                .Include(cm => cm.ClassSchedules)
                .Include(cm => cm.ClassAttendanceSummary)
                .Include(cm => cm.ClassAttendance)
                .Include(cm => cm.ClassAttendanceCheck)
                .FirstOrDefaultAsync(cm => cm.CM_Id == id);

            if (classManagement == null)
                return NotFound();

            await PopulateDropdowns(classManagement);
            return View(classManagement);
        }

        [HttpPost]
        [Route("ClassManagement/Delete/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "AdminPolicy")]
        public async Task<IActionResult> DeleteManage(int id)
        {
            try
            {
                var classManagement = await _db.ClassManagements
                    .FirstOrDefaultAsync(cm => cm.CM_Id == id);

                if (classManagement == null)
                {
                    TempData["ErrorMessage"] = "Class management record not found.";
                    return RedirectToAction("Index");
                }

                // Check for related records before deleting
                bool hasRelatedRecords = await _db.ClassAttendance
                    .AnyAsync(ca => ca.CM_Id == id) ||
                    await _db.ClassAttendanceCheck
                    .AnyAsync(cac => cac.CM_Id == id) ||
                    await _db.ClassAttendanceSummary
                    .AnyAsync(cas => cas.CM_Id == id) ||
                    await _db.ClassSchedules
                    .AnyAsync(cs => cs.CM_ID == id);

                if (hasRelatedRecords)
                {
                    TempData["ErrorMessage"] = "Cannot delete class management record with existing related records.";
                    return RedirectToAction("Index");
                }

                _db.ClassManagements.Remove(classManagement);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Class management record deleted successfully!";
            }
            catch (Exception ex)
            {
                // Consider logging the exception
                TempData["ErrorMessage"] = $"Error deleting class management record: {ex.Message}";
            }

            return RedirectToAction("Index");
        }


    }
}
