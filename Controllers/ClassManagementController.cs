using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models.ClassManagement;
using System.Linq;
using System.Threading.Tasks;

namespace SchoolSystem.Controllers
{
    public class ClassManagementController : Controller
    {
        private readonly AppDbContext _db;

        public ClassManagementController(AppDbContext db)
        {
            _db = db;
        }

        // 📌 แสดงรายการทั้งหมด
        public async Task<IActionResult> Index()
        {
            var classManagements = await _db.ClassManagements
                .Include(cm => cm.Class!).ThenInclude(c => c.GradeLevels!)
                .Include(cm => cm.Teacher!).ThenInclude(t => t.Profile!)
                .Include(cm => cm.Semester!)
                .Include(cm => cm.Course!)
                .AsNoTracking()
                .ToListAsync();

            return View(classManagements);
        }

        public IActionResult CreateManage()
        {
            LoadDropdownData();
            return View(new ClassManagement { CheckCount = 0 });
        }

        private void LoadDropdownData()
        {
            ViewData["Classes"] = _db.Classes
                .Include(c => c.GradeLevels)
                .Select(c => new SelectListItem
                {
                    Value = c.ClassId.ToString(),
                    Text = c.GradeLevels!.Name + "/" + c.ClassNumber
                }).ToList();

            ViewData["Teachers"] = _db.Teachers
                .Include(t => t.Profile)
                .Select(t => new SelectListItem
                {
                    Value = t.TeacherId.ToString(),
                    Text = t.Profile.FirstName + " " + t.Profile.LastName
                }).ToList();

            ViewData["Courses"] = _db.Course
                .Select(c => new SelectListItem
                {
                    Value = c.CourseId.ToString(),
                    Text = c.CourseName
                }).ToList();

            ViewData["Semesters"] = _db.Semesters
                .Select(s => new SelectListItem
                {
                    Value = s.SemesterID.ToString(),
                    Text = s.SemesterYear.ToString() + "/" + s.SemesterNumber.ToString()
                }).ToList();
        }

        private IActionResult ViewWithDropdowns(ClassManagement classManagement)
        {
            LoadDropdownData();
            return View(classManagement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateManage(ClassManagement classManagement)
        {
            if (ModelState.IsValid)
            {
                classManagement.CheckCount = 0;
                classManagement.CreateAt = DateTime.UtcNow;
                classManagement.UpdateAt = DateTime.UtcNow;

                if(classManagement.ScoringCriteria == null)
                {
                    classManagement.ScoringCriteria = "DefaultCriteria";
                }
                _db.ClassManagements.Add(classManagement);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Class management created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return ViewWithDropdowns(classManagement);
        }


        // 📌 GET: Edit
        public async Task<IActionResult> EditManage(int id)
        {
            var classManagement = await _db.ClassManagements.FindAsync(id);
            if (classManagement == null)
            {
                return NotFound();
            }

            return ViewWithDropdowns(classManagement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditManage(ClassManagement classManagement)
        {
            if (ModelState.IsValid)
            {
                var existingClassManagement = await _db.ClassManagements.FindAsync(classManagement.CM_Id);
                if (existingClassManagement == null)
                {
                    return NotFound();
                }

                // ✅ อัปเดตเฉพาะค่าที่สามารถแก้ไขได้
                existingClassManagement.ClassId = classManagement.ClassId;
                existingClassManagement.TeacherId = classManagement.TeacherId;
                existingClassManagement.CourseId = classManagement.CourseId;
                existingClassManagement.SemesterId = classManagement.SemesterId;
                existingClassManagement.ScoringCriteria = classManagement.ScoringCriteria;

                existingClassManagement.UpdateAt = DateTime.UtcNow; // ✅ อัปเดตเวลา

                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Class management updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            return ViewWithDropdowns(classManagement);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteManage(int id)
        {
            var classManagement = await _db.ClassManagements.FindAsync(id);
            if (classManagement == null)
            {
                TempData["ErrorMessage"] = "Record not found!";
                return Json(new { success = false });
            }

            _db.ClassManagements.Remove(classManagement);
            await _db.SaveChangesAsync();   

            TempData["SuccessMessage"] = "Class management deleted successfully!";
            return Json(new { success = true });
        }
        public async Task<IActionResult> ViewManage(int id)
        {
            var classManagement = await _db.ClassManagements
                .Include(cm => cm.Class!).ThenInclude(c => c.GradeLevels)
                .Include(cm => cm.Teacher!).ThenInclude(t => t.Profile)
                .Include(cm => cm.Semester)
                .Include(cm => cm.Course)
                .FirstOrDefaultAsync(cm => cm.CM_Id == id);

            if (classManagement == null)
            {
                return NotFound();
            }

            return View(classManagement);
        }

    }
}
