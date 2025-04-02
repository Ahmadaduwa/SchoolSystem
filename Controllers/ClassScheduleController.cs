using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models.ClassManagement;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSystem.Controllers
{
    [Authorize(Policy = "AcademicPolicyOrAdminPolicy")]
    public class ClassScheduleController : Controller
    {
        private readonly AppDbContext _db;

        public ClassScheduleController(AppDbContext db)
        {
            _db = db;
        }

        // 📌 แสดงตารางเรียนเฉพาะ CM_ID
        [HttpGet]
        [Route("ClassScheduler/{cmId}")]
        public async Task<IActionResult> IndexClassSchedule(int cmId)
        {
            Console.WriteLine($"CM_ID received in IndexClassSchedule: {cmId}"); // Debugging

            if (cmId == 0)
            {
                TempData["ErrorMessage"] = "Invalid CM_ID!";
                return RedirectToAction("Index", "ClassManagement");
            }

            var schedules = await _db.ClassSchedules
                .Where(cs => cs.CM_ID == cmId)
                .Include(cs => cs.ClassManagement)
                .AsNoTracking()
                .ToListAsync();

            ViewData["CM_ID"] = cmId;
            return View(schedules);
        }


        // 📌 GET: Create Schedule
        [HttpGet]
        [Route("ClassScheduler/Create/{cmId}")]
        public IActionResult Create(int cmId)
        {
            if (cmId == 0)
            {
                TempData["ErrorMessage"] = "Invalid CM_ID!";
                return RedirectToAction("Index", "ClassManagement");
            }

            ViewData["CM_ID"] = cmId;
            return View(new ClassSchedule { CM_ID = cmId });
        }

        // 📌 POST: Create Schedule
        [HttpPost]
        [Route("ClassScheduler/Create/{cmId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClassSchedule model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _db.ClassSchedules.Add(model);
                    await _db.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Schedule created successfully!";
                    return RedirectToAction(nameof(IndexClassSchedule), new { cmId = model.CM_ID });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred: " + ex.Message);
                }
            }

            ViewData["CM_ID"] = model.CM_ID;
            return View(model);
        }

        // 📌 GET: Edit Schedule
        [HttpGet]
        [Route("ClassScheduler/Edit/{Id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var schedule = await _db.ClassSchedules.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }

            ViewData["CM_ID"] = schedule.CM_ID;
            return View(schedule);
        }

        // 📌 POST: Edit Schedule
        [HttpPost]
        [Route("ClassScheduler/Edit/{Id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ClassSchedule model)
        {
            if (ModelState.IsValid)
            {
                var existingSchedule = await _db.ClassSchedules.FindAsync(model.ScheduleID);
                if (existingSchedule == null)
                {
                    return NotFound();
                }

                existingSchedule.DayOfWeek = model.DayOfWeek;
                existingSchedule.StartTime = model.StartTime;
                existingSchedule.EndTime = model.EndTime;
                existingSchedule.Status = model.Status;

                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Class schedule updated successfully!";
                return RedirectToAction(nameof(IndexClassSchedule), new { cmId = model.CM_ID });
            }

            ViewData["CM_ID"] = model.CM_ID;
            return View(model);
        }

        // 📌 POST: Delete Schedule
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var schedule = await _db.ClassSchedules.FindAsync(id);
            if (schedule == null)
            {
                return Json(new { success = false, message = "Schedule not found!" });
            }

            int cmId = schedule.CM_ID;
            _db.ClassSchedules.Remove(schedule);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Class schedule deleted successfully!";
            return Json(new { success = true, message = "Schedule deleted successfully!", cmId });
        }
    }
}
