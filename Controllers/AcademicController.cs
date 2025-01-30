using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models.CourseManagement;
using SchoolSystem.Models.ViewModels;


namespace SchoolSystem.Controllers
{
    [Authorize(Policy = "AcademicPolicy")]
    public class AcademicController : Controller
    {
        private readonly AppDbContext _db;

        public AcademicController(AppDbContext db)
        {
            _db = db;
        }



        //หน้าจัดการหลักสูตร
        [HttpGet]
        [Route("Course")]
        public IActionResult CourseManagement(string sortOrder, string filterStatus)
        {
            var coursesQuery = _db.Courses.AsQueryable();

            if (!string.IsNullOrEmpty(filterStatus))
            {
                coursesQuery = coursesQuery.Where(c => c.Status == filterStatus);
            }

            coursesQuery = sortOrder switch
            {
                "UpdateAtDesc" => coursesQuery.OrderByDescending(c => c.UpdateAt ?? DateTime.MaxValue),
                "UpdateAtAsc" => coursesQuery.OrderBy(c => c.UpdateAt ?? DateTime.MaxValue),
                "CreateAtDesc" => coursesQuery.OrderByDescending(c => c.CreateAt),
                "CreateAtAsc" => coursesQuery.OrderBy(c => c.CreateAt),
                "NameAsc" => coursesQuery.OrderBy(c => c.CourseName),
                "NameDesc" => coursesQuery.OrderByDescending(c => c.CourseName),
                _ => coursesQuery.OrderBy(c => c.Status == "Inactive").ThenByDescending(c => c.UpdateAt ?? DateTime.MaxValue)
            };

            List<Course> courses = coursesQuery.ToList();

            return View(courses);
        }

        [HttpGet]
        [Route("Course/Edit/{id:int}")] // เพิ่มการกำหนดประเภทพารามิเตอร์
        public IActionResult EditCourse(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }
            var obj = _db.Courses.Find(id);
            if (obj == null)
            {
                return NotFound(); // หากไม่พบ Course
            }
            return View(obj);
        }

        [HttpPost]
        [Route("Course/Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult EditCourse(Course course)
        {
            try
            {
                if (ModelState.IsValid)
                { 
                    var existingCourse = _db.Courses.Find(course.CourseId);
                    if (existingCourse == null)
                    {
                        return NotFound();
                    }

                    course.CreateAt = existingCourse.CreateAt;

                    course.UpdateAt = DateTime.UtcNow;

                    _db.Entry(existingCourse).CurrentValues.SetValues(course);
                    _db.SaveChanges();

                    return RedirectToAction("CourseManagement");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError("", "เกิดข้อผิดพลาดในการบันทึกข้อมูล");
            }

            return View(course);
        }

        [HttpGet]
        [Route("Course/Add")]
        public IActionResult AddCourse()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Course/Add")]
        public IActionResult AddCourse(Course course)
        {
            if (ModelState.IsValid)
            {
                course.CreateAt = DateTime.UtcNow;
                course.UpdateAt = DateTime.UtcNow;

                _db.Courses.Add(course);
                _db.SaveChanges();

                return RedirectToAction("CourseManagement");
            }

            return View(course); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Course/Delete/{id:int}")]
        public IActionResult DeleteCourse(int id)
        {
            var course = _db.Courses.Find(id);
            if (course == null)
            {
                return NotFound(); 
            }

            _db.Courses.Remove(course);
            _db.SaveChanges(); 

            return RedirectToAction("CourseManagement"); 
        }


        //จัดการกิจกรรมหลักสูตร
        [HttpGet]
        [Route("Course/Activity/{id:int}")]
        public IActionResult CourseActivity(int id, string sortOrder, string filterStatus)
        {
            var course = _db.Courses.FirstOrDefault(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            var activitiesQuery = _db.ExtracurricularActivities
                .Where(ea => ea.CourseId == id)
                .Include(ea => ea.Activity)
                .Select(ea => ea.Activity)
                .Where(a => a != null)
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

            var courseActivityViewModel = new CourseActivityViewModel
            {
                CourseId = course.CourseId,
                Course_Code = course.Course_Code,
                CourseName = course.CourseName,
                Activities = activities
            };

            return View(courseActivityViewModel);
        }
        [HttpGet]
        [Route("Course/Activity/Add/{id:int}")]
        public IActionResult AddActivity(int id)
        {
            var course = _db.Courses.FirstOrDefault(c => c.CourseId == id);
            if (course == null)
            {
                return NotFound();
            }

            // ดึงกิจกรรมที่ถูกเลือกไปแล้ว
            var selectedActivities = _db.ExtracurricularActivities
                .Where(ea => ea.CourseId == id)
                .Select(ea => ea.ActivityId)
                .ToList();

            // กรองเฉพาะกิจกรรมที่ยังไม่ถูกเลือก
            ViewBag.Activities = _db.Activities
                .Where(a => a.Status == "Active" && !selectedActivities.Contains(a.ActivityId))
                .Select(a => new { a.ActivityId, a.ActivityName })
                .ToList();

            var model = new ExtracurricularActivity { CourseId = id };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Course/Activity/Add/{id:int}")]
        public IActionResult AddActivity(int id, ExtracurricularActivity extracurricularActivity)
        {
            if (extracurricularActivity.CourseId == 0 || extracurricularActivity.ActivityId == 0)
            {
                TempData["ErrorMessage"] = "กรุณาเลือกหลักสูตรและกิจกรรมที่ถูกต้อง!";
                return View(extracurricularActivity);
            }

            if (ModelState.IsValid)
            {
                extracurricularActivity.CreateAt = DateTime.UtcNow;
                extracurricularActivity.Status = "Active";

                _db.ExtracurricularActivities.Add(extracurricularActivity);
                _db.SaveChanges();

                TempData["SuccessMessage"] = "เพิ่มกิจกรรมสำเร็จ!";
                return RedirectToAction("AddActivity", new { id = extracurricularActivity.CourseId });
            }

            TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการเพิ่มกิจกรรม!";
            return View(extracurricularActivity);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Academic/DeleteActivity")]
        public IActionResult DeleteActivity(int activityId, int courseId)
        {
            var activityToRemove = _db.ExtracurricularActivities
                .FirstOrDefault(ea => ea.ActivityId == activityId && ea.CourseId == courseId);

            if (activityToRemove == null)
            {
                return NotFound();
            }

            _db.ExtracurricularActivities.Remove(activityToRemove);
            _db.SaveChanges();

            return RedirectToAction("CourseActivity", new { id = courseId });
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
