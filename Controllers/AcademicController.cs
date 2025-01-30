using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models.CurriculumManagement;
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
        [Route("Curriculum")]
        public IActionResult CurriculumManagement(string sortOrder, string filterStatus)
        {
            var CurriculumQuery = _db.Curriculum.AsQueryable();

            if (!string.IsNullOrEmpty(filterStatus))
            {
                CurriculumQuery = CurriculumQuery.Where(c => c.Status == filterStatus);
            }

            CurriculumQuery = sortOrder switch
            {
                "UpdateAtDesc" => CurriculumQuery.OrderByDescending(c => c.UpdateAt ?? DateTime.MaxValue),
                "UpdateAtAsc" => CurriculumQuery.OrderBy(c => c.UpdateAt ?? DateTime.MaxValue),
                "CreateAtDesc" => CurriculumQuery.OrderByDescending(c => c.CreateAt),
                "CreateAtAsc" => CurriculumQuery.OrderBy(c => c.CreateAt),
                "NameAsc" => CurriculumQuery.OrderBy(c => c.CurriculumName),
                "NameDesc" => CurriculumQuery.OrderByDescending(c => c.CurriculumName),
                _ => CurriculumQuery.OrderBy(c => c.Status == "Inactive").ThenByDescending(c => c.UpdateAt ?? DateTime.MaxValue)
            };

            List<Curriculum> Curriculum = CurriculumQuery.ToList();

            return View(Curriculum);
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


        //จัดการกิจกรรมหลักสูตร
        [HttpGet]
        [Route("Curriculum/Activity/{id:int}")]
        public IActionResult CurriculumActivity(int id, string sortOrder, string filterStatus)
        {
            var Curriculum = _db.Curriculum.FirstOrDefault(c => c.CurriculumId == id);

            if (Curriculum == null)
            {
                return NotFound();
            }

            var activitiesQuery = _db.ExtracurricularActivities
                .Where(ea => ea.CurriculumId == id)
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

            var CurriculumActivityViewModel = new CurriculumActivityViewModel
            {
                CurriculumId = Curriculum.CurriculumId,
                Curriculum_Code = Curriculum.Curriculum_Code,
                CurriculumName = Curriculum.CurriculumName,
                Activities = activities
            };

            return View(CurriculumActivityViewModel);
        }
        [HttpGet]
        [Route("Curriculum/Activity/Add/{id:int}")]
        public IActionResult AddActivity(int id)
        {
            var Curriculum = _db.Curriculum.FirstOrDefault(c => c.CurriculumId == id);
            if (Curriculum == null)
            {
                return NotFound();
            }

            // ดึงกิจกรรมที่ถูกเลือกไปแล้ว
            var selectedActivities = _db.ExtracurricularActivities
                .Where(ea => ea.CurriculumId == id)
                .Select(ea => ea.ActivityId)
                .ToList();

            // กรองเฉพาะกิจกรรมที่ยังไม่ถูกเลือก
            ViewBag.Activities = _db.Activities
                .Where(a => a.Status == "Active" && !selectedActivities.Contains(a.ActivityId))
                .Select(a => new { a.ActivityId, a.ActivityName })
                .ToList();

            var model = new ExtracurricularActivity { CurriculumId = id };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Curriculum/Activity/Add/{id:int}")]
        public IActionResult AddActivity(int id, ExtracurricularActivity extracurricularActivity)
        {
            if (extracurricularActivity.CurriculumId == 0 || extracurricularActivity.ActivityId == 0)
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
                return RedirectToAction("AddActivity", new { id = extracurricularActivity.CurriculumId });
            }

            TempData["ErrorMessage"] = "เกิดข้อผิดพลาดในการเพิ่มกิจกรรม!";
            return View(extracurricularActivity);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route("Academic/DeleteActivity")]
        public IActionResult DeleteActivity(int activityId, int CurriculumId)
        {
            var activityToRemove = _db.ExtracurricularActivities
                .FirstOrDefault(ea => ea.ActivityId == activityId && ea.CurriculumId == CurriculumId);

            if (activityToRemove == null)
            {
                return NotFound();
            }

            _db.ExtracurricularActivities.Remove(activityToRemove);
            _db.SaveChanges();

            return RedirectToAction("CurriculumActivity", new { id = CurriculumId });
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
