using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models.ClassManagement;

namespace SchoolSystem.Controllers
{
    //[Authorize(Policy = "AcademicPolicy")]
    public class ClassController : Controller
    {
        private readonly AppDbContext _db;

        public ClassController(AppDbContext db)
        {
            _db = db;
        }
        //แสดงห้องเรียน
        public IActionResult IndexClass()
        {
            var classes = _db.Classes.Include(c => c.GradeLevels).ToList();
            return View(classes);
        }

        //เพิ่มห้องเรียน
        public IActionResult CreateClass()
        {
            ViewBag.GradeLevels = _db.GradeLevels.ToList();
            return View();
        }

        [HttpPost]
        public IActionResult CreateClass(Class newClass)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.GradeLevels = _db.GradeLevels.ToList();
                return View(newClass);
            }

            try
            {
                _db.Classes.Add(newClass);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Class created successfully!";
                return RedirectToAction("IndexClass");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Database Error: {ex.Message}");
            }

            ViewBag.GradeLevels = _db.GradeLevels.ToList();
            return View(newClass);
        }

        // แสดงหน้าแก้ไขคลาส
        // แสดงหน้าแก้ไขคลาส
        public IActionResult EditClass(int id)
        {
            var classToEdit = _db.Classes.Find(id);
            if (classToEdit == null)
            {
                return NotFound();
            }

            ViewBag.GradeLevels = _db.GradeLevels.ToList(); // ✅ โหลด Grade Levels สำหรับ Dropdown
            return View(classToEdit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditClass(Class updatedClass)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.GradeLevels = _db.GradeLevels.ToList();
                return View(updatedClass);
            }

            try
            {
                _db.Classes.Update(updatedClass);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Class updated successfully!";
                return RedirectToAction("IndexClass");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }

            return View(updatedClass);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteClass(int id)
        {
            try
            {
                var classToDelete = _db.Classes
                    .Include(c => c.GradeLevels) // ✅ ดึงข้อมูล GradeLevel เพื่อป้องกันข้อผิดพลาด
                    .FirstOrDefault(c => c.ClassId == id);

                if (classToDelete == null)
                {
                    TempData["ErrorMessage"] = "Class not found.";
                    return RedirectToAction("IndexClass");
                }

                _db.Classes.Remove(classToDelete);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Class deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting class: {ex.Message}";
            }

            return RedirectToAction("IndexClass");
        }

    }
}
