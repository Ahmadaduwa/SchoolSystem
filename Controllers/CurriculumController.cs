using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Helpers;
using SchoolSystem.Models.ClassManagement;
using SchoolSystem.Models.CurriculumManagement;
using SchoolSystem.Models.ViewModels;


namespace SchoolSystem.Controllers
{
    [Authorize(Policy = "AcademicPolicyOrAdminPolicy")]
    public class CurriculumController : Controller
    {
        private readonly AppDbContext _db;

        public CurriculumController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Route("Curriculums")]
        public async Task<IActionResult> CurriculumManagement(int? pageNumber, string searchString, string sortOrder, string statusFilter)
        {
            // Store sorting and filtering data in ViewData
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CodeSortParam"] = string.IsNullOrEmpty(sortOrder) ? "code_desc" : "";
            ViewData["NameSortParam"] = sortOrder == "name" ? "name_desc" : "name";
            ViewData["StatusSortParam"] = sortOrder == "status" ? "status_desc" : "status";
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = statusFilter;

            // Get distinct statuses for the filter dropdown
            var statuses = await _db.Curriculum
                .Select(c => c.Status)
                .Distinct()
                .ToListAsync();
            ViewData["Statuses"] = statuses
                .Select(s => new SelectListItem { Value = s, Text = s })
                .ToList();

            // Build the initial query
            var curriculumsQuery = _db.Curriculum.AsNoTracking();

            // Search by Curriculum_Code, CurriculumName, or Description
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                curriculumsQuery = curriculumsQuery.Where(c =>
                    c.Curriculum_Code.Contains(searchString) ||
                    c.CurriculumName.Contains(searchString) ||
                    (c.Description != null && c.Description.Contains(searchString)));
            }

            // Filter by Status
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                curriculumsQuery = curriculumsQuery.Where(c => c.Status == statusFilter);
            }

            // Sort the data
            curriculumsQuery = sortOrder switch
            {
                "code_desc" => curriculumsQuery.OrderByDescending(c => c.Curriculum_Code),
                "name" => curriculumsQuery.OrderBy(c => c.CurriculumName),
                "name_desc" => curriculumsQuery.OrderByDescending(c => c.CurriculumName),
                "status" => curriculumsQuery.OrderBy(c => c.Status),
                "status_desc" => curriculumsQuery.OrderByDescending(c => c.Status),
                _ => curriculumsQuery.OrderBy(c => c.Curriculum_Code),
            };

            // Pagination
            int pageSize = 10;
            var totalItems = await curriculumsQuery.CountAsync();
            ViewData["TotalItems"] = totalItems;
            var pagedCurriculums = await PaginatedList<Curriculum>.CreateAsync(curriculumsQuery, pageNumber ?? 1, pageSize);

            return View(pagedCurriculums);
        }

        [HttpGet]
        [Route("Curriculums/Edit/{id:int}")] // เพิ่มการกำหนดประเภทพารามิเตอร์
        public IActionResult EditCurriculum(int id)
        {
            var curriculum = _db.Curriculum.Find(id);
            if (curriculum == null)
            {
                return NotFound();
            }
            return View(curriculum);
        }

        [HttpPost]
        [Route("Curriculums/Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult EditCurriculum(Curriculum updatedCurriculum)
        {
            // Find the existing curriculum in the database
            var existingCurriculum = _db.Curriculum.Find(updatedCurriculum.CurriculumId);

            if (existingCurriculum == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(updatedCurriculum);
            }

            try
            {
                // Preserve the original CreateAt timestamp
                updatedCurriculum.CreateAt = existingCurriculum.CreateAt;

                // Update the UpdateAt timestamp to current time (UTC)
                updatedCurriculum.UpdateAt = DateTime.UtcNow;

                // Detach the existing entity to avoid tracking conflicts
                _db.Entry(existingCurriculum).State = EntityState.Detached;

                // Update the entire entity
                _db.Curriculum.Update(updatedCurriculum);
                _db.SaveChanges();

                TempData["SuccessMessage"] = "Curriculum updated successfully!";
                return RedirectToAction("CurriculumManagement");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating curriculum: {ex.Message}");
                return View(updatedCurriculum);
            }
        }

        [HttpGet]
        [Route("Curriculums/Add")]
        public IActionResult AddCurriculum()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Curriculums/Add")]
        public IActionResult AddCurriculum(Curriculum newCurriculum)
        {
            if (!ModelState.IsValid)
            {
                return View(newCurriculum);
            }

            try
            {
                _db.Curriculum.Add(newCurriculum);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Curriculum created successfully!";
                return RedirectToAction("CurriculumManagement");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Database Error: {ex.Message}");
            }

            return View(newCurriculum);
        }

        [HttpGet]
        [Route("Curriculums/Details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var curriculum = await _db.Curriculum
                .FirstOrDefaultAsync(m => m.CurriculumId == id);

            if (curriculum == null)
                return NotFound();

            return View(curriculum);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Curriculums/Delete/{id:int}")]
        public IActionResult DeleteCurriculum(int id)
        {
            var Curriculum = _db.Curriculum.Find(id);
            if (Curriculum == null)
            {
                return NotFound(); 
            }

            _db.Curriculum.Remove(Curriculum);
            _db.SaveChanges(); 

            return RedirectToAction("CurriculumManagement"); 
        }



        // 📌 จัดการกิจกรรมของหลักสูตร (แสดงรายการ + เพิ่มกิจกรรม)
        [HttpGet]
        [Route("Curriculums/Activity/{id:int}")]
        public IActionResult ManageCurriculumActivities(int id, string sortOrder)
        {
            var curriculum = _db.Curriculum.FirstOrDefault(c => c.CurriculumId == id);
            if (curriculum == null)
            {
                return NotFound();
            }

            // 📌 ดึงกิจกรรมที่เพิ่มไปแล้ว
            var activitiesQuery = _db.ExtracurricularActivities
                .Where(ea => ea.CurriculumId == id)
                .Include(ea => ea.Activity)
                .Select(ea => ea.Activity)
                .AsQueryable();

            activitiesQuery = sortOrder switch
            {
                "CreateAtDesc" => activitiesQuery.OrderByDescending(a => a.CreateAt),
                "CreateAtAsc" => activitiesQuery.OrderBy(a => a.CreateAt),
                "NameAsc" => activitiesQuery.OrderBy(a => a.ActivityName),
                "NameDesc" => activitiesQuery.OrderByDescending(a => a.ActivityName),
                _ => activitiesQuery.OrderByDescending(a => a.CreateAt)
            };

            var activities = activitiesQuery.ToList();

            // 📌 กรองกิจกรรมที่ยังไม่ได้เพิ่มเข้าไป
            var selectedActivities = _db.ExtracurricularActivities
                .Where(ea => ea.CurriculumId == id)
                .Select(ea => ea.ActivityId)
                .ToList();

            ViewBag.Activities = _db.Activities
                .Where(a => a.Status == "Active" && a.ActivityType == "Special" && !selectedActivities.Contains(a.ActivityId))
                .Select(a => new { a.ActivityId, a.ActivityName })
                .ToList();

            var model = new CurriculumActivityViewModel
            {
                CurriculumId = curriculum.CurriculumId,
                CurriculumName = curriculum.CurriculumName,
                Activities = activities
            };

            return View(model);
        }

        // 📌 เพิ่มกิจกรรมในหลักสูตร
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Curriculums/Activity/Add")]
        public IActionResult AddActivity(int CurriculumId, int ActivityId)
        {
            if (CurriculumId == 0 || ActivityId == 0)
            {
                TempData["ErrorMessage"] = "กรุณาเลือกกิจกรรมที่ถูกต้อง!";
                return RedirectToAction("ManageCurriculumActivities", new { id = CurriculumId });
            }

            // ตรวจสอบว่ามีอยู่แล้วหรือไม่
            bool isExist = _db.ExtracurricularActivities.Any(ea => ea.CurriculumId == CurriculumId && ea.ActivityId == ActivityId);
            if (isExist)
            {
                TempData["ErrorMessage"] = "กิจกรรมนี้ถูกเพิ่มไปแล้ว!";
                return RedirectToAction("ManageCurriculumActivities", new { id = CurriculumId });
            }

            var newActivity = new ExtracurricularActivity
            {
                CurriculumId = CurriculumId,
                ActivityId = ActivityId,
                CreateAt = DateTime.UtcNow,
                Status = "Active"
            };

            _db.ExtracurricularActivities.Add(newActivity);
            _db.SaveChanges();

            TempData["SuccessMessage"] = "เพิ่มกิจกรรมสำเร็จ!";
            return RedirectToAction("ManageCurriculumActivities", new { id = CurriculumId });
        }

        // 📌 ลบกิจกรรมออกจากหลักสูตร
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Curriculums/Activity/Delete")]
        public IActionResult DeleteActivity(int activityId, int CurriculumId)
        {
            var activityToRemove = _db.ExtracurricularActivities
                .FirstOrDefault(ea => ea.ActivityId == activityId && ea.CurriculumId == CurriculumId);

            if (activityToRemove == null)
            {
                TempData["ErrorMessage"] = "ไม่พบกิจกรรมที่ต้องการลบ!";
                return RedirectToAction("ManageCurriculumActivities", new { id = CurriculumId });
            }

            _db.ExtracurricularActivities.Remove(activityToRemove);
            _db.SaveChanges();

            TempData["SuccessMessage"] = "ลบกิจกรรมสำเร็จ!";
            return RedirectToAction("ManageCurriculumActivities", new { id = CurriculumId });
        }


        [HttpGet]
        [Route("Curriculums/Courses/{id:int}")]
        public IActionResult ManageCourses(int id)
        {
            var curriculum = _db.Curriculum.FirstOrDefault(c => c.CurriculumId == id);
            if (curriculum == null)
            {
                return NotFound();
            }

            // ดึงข้อมูล GradeLevels และ Courses จาก Database
            ViewBag.GradeLevels = _db.GradeLevels.Where(cc => cc.Status == "Active").ToList();
            ViewBag.Courses = _db.Course.Where(cc => cc.Status == "Active").ToList();

            var model = new ManageCoursesViewModel
            {
                CurriculumId = curriculum.CurriculumId,
                CurriculumName = curriculum.CurriculumName,
                ElectiveCourses = _db.ElectiveCourses
                    .Where(ec => ec.CurriculumId == id)
                    .Include(ec => ec.Course)
                    .Include(ec => ec.GradeLevel)
                    .ToList(),

                CompulsoryCourses = _db.CompulsoryCourses
                    .Where(cc => cc.CurriculumId == id)
                    .Include(cc => cc.Course)
                    .Include(cc => cc.GradeLevel)
                    .ToList(),

                CompulsoryElectiveCourses = _db.CompulsoryElectiveCourses
                    .Where(cec => cec.CurriculumId == id)
                    .Include(cec => cec.Course)
                    .Include(cec => cec.GradeLevel)
                    .ToList()
            };

            return View(model);
        }

        // เพิ่มรายวิชา
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Curriculums/Courses/Add")]
        public IActionResult AddCourse(int curriculumId, string courseType, int gradeLevelId, int courseId)
        {
            if (string.IsNullOrEmpty(courseType) || curriculumId == 0 || gradeLevelId == 0 || courseId == 0)
            {
                TempData["ErrorMessage"] = "ข้อมูลไม่ถูกต้อง!";
                return RedirectToAction("ManageCourses", new { id = curriculumId });
            }

            if (courseType == "Elective")
            {
                _db.ElectiveCourses.Add(new ElectiveCourse { CurriculumId = curriculumId, GradeLevelId = gradeLevelId, CourseId = courseId });
            }
            else if (courseType == "Compulsory")
            {
                _db.CompulsoryCourses.Add(new CompulsoryCourse { CurriculumId = curriculumId, GradeLevelId = gradeLevelId, CourseId = courseId });
            }
            else if (courseType == "CompulsoryElective")
            {
                _db.CompulsoryElectiveCourses.Add(new CompulsoryElectiveCourse { CurriculumId = curriculumId, GradeLevelId = gradeLevelId, CourseId = courseId });
            }

            _db.SaveChanges();
            TempData["SuccessMessage"] = "เพิ่มรายวิชาสำเร็จ!";
            return RedirectToAction("ManageCourses", new { id = curriculumId });
        }

        // ลบรายวิชา
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Curriculums/Courses/Delete")]
        public IActionResult DeleteCourse(int courseId, int curriculumId, string courseType)
        {
            if (string.IsNullOrEmpty(courseType) || curriculumId == 0 || courseId == 0)
            {
                TempData["ErrorMessage"] = "ข้อมูลไม่ถูกต้อง!";
                return RedirectToAction("ManageCourses", new { id = curriculumId });
            }

            if (courseType == "Elective")
            {
                var course = _db.ElectiveCourses.FirstOrDefault(ec => ec.CourseId == courseId && ec.CurriculumId == curriculumId);
                if (course != null) _db.ElectiveCourses.Remove(course);
            }
            else if (courseType == "Compulsory")
            {
                var course = _db.CompulsoryCourses.FirstOrDefault(cc => cc.CourseId == courseId && cc.CurriculumId == curriculumId);
                if (course != null) _db.CompulsoryCourses.Remove(course);
            }
            else if (courseType == "CompulsoryElective")
            {
                var course = _db.CompulsoryElectiveCourses.FirstOrDefault(cec => cec.CourseId == courseId && cec.CurriculumId == curriculumId);
                if (course != null) _db.CompulsoryElectiveCourses.Remove(course);
            }

            _db.SaveChanges();
            TempData["SuccessMessage"] = "ลบรายวิชาสำเร็จ!";
            return RedirectToAction("ManageCourses", new { id = curriculumId });
        }

        public IActionResult SearchActivities(string searchTerm)
        {
            var activities = _db.Activities
                .Where(a => a.Status == "Active" && (string.IsNullOrEmpty(searchTerm) || a.ActivityName.Contains(searchTerm)))
                .OrderBy(a => a.ActivityName)
                .Select(a => new { activityId = a.ActivityId, activityName = a.ActivityName })
                .Take(10)
                .ToList();

            return Json(activities);
        }

        public IActionResult GetActivityDetails(int id)
        {
            var activity = _db.Activities
                .Where(a => a.ActivityId == id)
                .Select(a => new {
                    activityId = a.ActivityId,
                    activityName = a.ActivityName,
                    description = a.Description,
                    createAt = a.CreateAt
                })
                .FirstOrDefault();

            if (activity == null)
            {
                return NotFound();
            }

            return Json(activity);
        }


    }
}
