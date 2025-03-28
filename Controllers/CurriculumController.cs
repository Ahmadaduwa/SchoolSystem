using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
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



        //หน้าจัดการหลักสูตร
        [HttpGet]
        [Route("Curriculum")]
        public IActionResult CurriculumManagement(string sortOrder, string filterStatus)
        {
            var CurriculumsQuery = _db.Curriculum.AsQueryable();

            if (!string.IsNullOrEmpty(filterStatus))
            {
                CurriculumsQuery = CurriculumsQuery.Where(c => c.Status == filterStatus);
            }

            CurriculumsQuery = sortOrder switch
            {
                "UpdateAtDesc" => CurriculumsQuery.OrderByDescending(c => c.UpdateAt ?? DateTime.MaxValue),
                "UpdateAtAsc" => CurriculumsQuery.OrderBy(c => c.UpdateAt ?? DateTime.MaxValue),
                "CreateAtDesc" => CurriculumsQuery.OrderByDescending(c => c.CreateAt),
                "CreateAtAsc" => CurriculumsQuery.OrderBy(c => c.CreateAt),
                "NameAsc" => CurriculumsQuery.OrderBy(c => c.CurriculumName),
                "NameDesc" => CurriculumsQuery.OrderByDescending(c => c.CurriculumName),
                _ => CurriculumsQuery.OrderBy(c => c.Status == "Inactive").ThenByDescending(c => c.UpdateAt ?? DateTime.MaxValue)
            };

            List<Curriculum> Curriculums = CurriculumsQuery.ToList();

            return View(Curriculums);
        }

        [HttpGet]
        [Route("Curriculum/Edit/{id:int}")] // เพิ่มการกำหนดประเภทพารามิเตอร์
        public IActionResult EditCurriculum(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }
            var obj = _db.Curriculum.Find(id);
            if (obj == null)
            {
                return NotFound(); // หากไม่พบ Curriculum
            }
            return View(obj);
        }

        [HttpPost]
        [Route("Curriculum/Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult EditCurriculum(Curriculum Curriculum)
        {
            try
            {
                if (ModelState.IsValid)
                { 
                    var existingCurriculum = _db.Curriculum.Find(Curriculum.CurriculumId);
                    if (existingCurriculum == null)
                    {
                        return NotFound();
                    }

                    Curriculum.CreateAt = existingCurriculum.CreateAt;

                    Curriculum.UpdateAt = DateTime.UtcNow;

                    _db.Entry(existingCurriculum).CurrentValues.SetValues(Curriculum);
                    _db.SaveChanges();

                    return RedirectToAction("CurriculumManagement");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError("", "เกิดข้อผิดพลาดในการบันทึกข้อมูล");
            }

            return View(Curriculum);
        }

        [HttpGet]
        [Route("Curriculum/Add")]
        public IActionResult AddCurriculum()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Curriculum/Add")]
        public IActionResult AddCurriculum(Curriculum Curriculum)
        {
            if (ModelState.IsValid)
            {
                Curriculum.CreateAt = DateTime.UtcNow;
                Curriculum.UpdateAt = DateTime.UtcNow;

                _db.Curriculum.Add(Curriculum);
                _db.SaveChanges();

                return RedirectToAction("CurriculumManagement");
            }

            return View(Curriculum); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Curriculum/Delete/{id:int}")]
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
        [Route("Curriculum/Activity/{id:int}")]
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
                .Where(a => a.Status == "Active" && !selectedActivities.Contains(a.ActivityId))
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
        [Route("Curriculum/Activity/Add")]
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
        [Route("Curriculum/Activity/Delete")]
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
        [Route("Curriculum/Courses/{id:int}")]
        public IActionResult ManageCourses(int id)
        {
            var curriculum = _db.Curriculum.FirstOrDefault(c => c.CurriculumId == id);
            if (curriculum == null)
            {
                return NotFound();
            }

            // ดึงข้อมูล GradeLevels และ Courses จาก Database
            ViewBag.GradeLevels = _db.GradeLevels.ToList();
            ViewBag.Courses = _db.Course.ToList();

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
        [Route("Curriculum/Courses/Add")]
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
        [Route("Curriculum/Courses/Delete")]
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



        //funtion
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
