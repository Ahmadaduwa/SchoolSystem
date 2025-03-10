using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models.ActivityManagement;

namespace SchoolSystem.Controllers
{
    //[Authorize(Policy = "AcademicPolicy")]
    public class ActivityController : Controller
    {
        private readonly AppDbContext _db;

        public ActivityController(AppDbContext db)
        {
            _db = db;
        }
        // แสดงกิจกรรม
        public IActionResult IndexActivity()
        {
            var activities = _db.Activities.ToList();
            return View(activities);
        }

        // เพิ่มกิจกรรม
        public IActionResult CreateActivity()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateActivity(Activity newActivity)
        {
            if (!ModelState.IsValid)
            {
                return View(newActivity);
            }

            try
            {
                _db.Activities.Add(newActivity);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Activity created successfully!";
                return RedirectToAction("IndexActivity");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Database Error: {ex.Message}");
            }

            return View(newActivity);
        }

        // แสดงหน้าแก้ไขกิจกรรม
        public IActionResult EditActivity(int id)
        {
            var activityToEdit = _db.Activities.Find(id);
            if (activityToEdit == null)
            {
                return NotFound();
            }
            return View(activityToEdit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditActivity(Activity updatedActivity)
        {
            if (!ModelState.IsValid)
            {
                return View(updatedActivity);
            }

            try
            {
                _db.Activities.Update(updatedActivity);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Activity updated successfully!";
                return RedirectToAction("IndexActivity");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }

            return View(updatedActivity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteActivity(int id)
        {
            try
            {
                var activityToDelete = _db.Activities
                    .FirstOrDefault(a => a.ActivityId == id);

                if (activityToDelete == null)
                {
                    TempData["ErrorMessage"] = "Activity not found.";
                    return RedirectToAction("IndexActivity");
                }

                _db.Activities.Remove(activityToDelete);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Activity deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting activity: {ex.Message}";
            }

            return RedirectToAction("IndexActivity");
        }
    }
}
