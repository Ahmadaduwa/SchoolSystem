using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchoolSystem.Data;
using SchoolSystem.Models.ClassManagement;
using System;

namespace SchoolSystem.Controllers
{
    public class GeneralController : Controller
    {
        private readonly ApplicationDBContext _db;

        public GeneralController(ApplicationDBContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreateClass()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateClass(Class newClass)
        {
            if (ModelState.IsValid)
            {
                _db.Classes.Add(newClass);
                try
                {
                    _db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
                TempData["SuccessMessage"] = "Class created successfully!";
                return RedirectToAction("IndexClass");
            }
            return View(newClass);
        }


        public IActionResult EditClass(int id)
        {
            var classToEdit = _db.Classes.Find(id);
            if (classToEdit == null)
            {
                return NotFound();
            }
            return View(classToEdit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditClass(Class updatedClass)
        {
            if (ModelState.IsValid)
            {
                _db.Classes.Update(updatedClass);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Class updated successfully!";
                return RedirectToAction("Index");
            }
            return View(updatedClass);
        }

        public IActionResult DeleteClass(int id)
        {
            var classToDelete = _db.Classes.Find(id);
            if (classToDelete == null)
            {
                return NotFound();
            }
            return View(classToDelete);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmedClass(int id)
        {
            var classToDelete = _db.Classes.Find(id);
            if (classToDelete != null)
            {
                _db.Classes.Remove(classToDelete);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Class deleted successfully!";
            }
            return RedirectToAction("Index");
        }



        public async Task<IActionResult> IndexManage()
        {
            var classManagements = await _db.ClassManagements
                .Include(cm => cm.Class)
                .Include(cm => cm.Teacher)
                .Include(cm => cm.Semester)
                .Include(cm => cm.Subject)
                .ToListAsync();
            return View(classManagements);
        }

        // GET: Create
        public IActionResult CreateManage()
        {
            ViewData["Classes"] = _db.Classes.ToList();
            ViewData["Teachers"] = _db.Teachers.ToList();
            ViewData["Semesters"] = _db.Semesters.ToList();
            ViewData["Subjects"] = _db.Subjects.ToList();
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateManage(ClassManagement classManagement)
        {
            if (ModelState.IsValid)
            {
                _db.ClassManagements.Add(classManagement);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Class management record created successfully!";
                return RedirectToAction("IndexManage");
            }

            ViewData["Classes"] = _db.Classes.ToList();
            ViewData["Teachers"] = _db.Teachers.ToList();
            ViewData["Semesters"] = _db.Semesters.ToList();
            ViewData["Subjects"] = _db.Subjects.ToList();
            return View(classManagement);
        }

        // GET: Edit
        public async Task<IActionResult> EditManage(int id)
        {
            var classManagement = await _db.ClassManagements.FindAsync(id);
            if (classManagement == null)
            {
                return NotFound();
            }

            ViewData["Classes"] = _db.Classes.ToList();
            ViewData["Teachers"] = _db.Teachers.ToList();
            ViewData["Semesters"] = _db.Semesters.ToList();
            ViewData["Subjects"] = _db.Subjects.ToList();
            return View(classManagement);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditManage(ClassManagement classManagement)
        {
            if (ModelState.IsValid)
            {
                _db.ClassManagements.Update(classManagement);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Class management record updated successfully!";
                return RedirectToAction("Index");
            }

            ViewData["Classes"] = _db.Classes.ToList();
            ViewData["Teachers"] = _db.Teachers.ToList();
            ViewData["Semesters"] = _db.Semesters.ToList();
            ViewData["Subjects"] = _db.Subjects.ToList();
            return View(classManagement);
        }

        // GET: Delete
        public async Task<IActionResult> DeleteManage(int id)
        {
            var classManagement = await _db.ClassManagements
                .Include(cm => cm.Class)
                .Include(cm => cm.Teacher)
                .Include(cm => cm.Semester)
                .Include(cm => cm.Subject)
                .FirstOrDefaultAsync(cm => cm.CM_ID == id);

            if (classManagement == null)
            {
                return NotFound();
            }

            return View(classManagement);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedManage(int id)
        {
            var classManagement = await _db.ClassManagements.FindAsync(id);
            if (classManagement != null)
            {
                _db.ClassManagements.Remove(classManagement);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Class management record deleted successfully!";
            }

            return RedirectToAction("Index");
        }

        // GET: Details
        public async Task<IActionResult> DetailsManage(int id)
        {
            var classManagement = await _db.ClassManagements
                .Include(cm => cm.Class)
                .Include(cm => cm.Teacher)
                .Include(cm => cm.Semester)
                .Include(cm => cm.Subject)
                .FirstOrDefaultAsync(cm => cm.CM_ID == id);

            if (classManagement == null)
            {
                return NotFound();
            }

            return View(classManagement);
        }

        public IActionResult IndexTeacher()
        {
            var teachers = _db.Teachers.ToList();
            return View(teachers);
        }

        // GET: Teacher/Create
        public IActionResult CreateTeacher()
        {
            return View();
        }

        // POST: Teacher/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateTeacher(Teacher model)
        {
            if (ModelState.IsValid)
            {
                _db.Teachers.Add(model);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Teacher created successfully!";
                return RedirectToAction("IndexTeacher");
            }
            return View(model);
        }

        // GET: Teacher/Edit/{id}
        public IActionResult EditTeacher(int id)
        {
            var teacher = _db.Teachers.Find(id);
            if (teacher == null)
            {
                return NotFound();
            }
            return View(teacher);
        }

        // POST: Teacher/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditTeacher(Teacher model)
        {
            if (ModelState.IsValid)
            {
                _db.Teachers.Update(model);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Teacher updated successfully!";
                return RedirectToAction("IndexTeacher");
            }
            return View(model);
        }

        // GET: Teacher/Delete/{id}
        public IActionResult DeleteTeacher(int id)
        {
            var teacher = _db.Teachers.Find(id);
            if (teacher == null)
            {
                return NotFound();
            }
            return View(teacher);
        }

        // POST: Teacher/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var teacher = _db.Teachers.Find(id);
            if (teacher != null)
            {
                _db.Teachers.Remove(teacher);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Teacher deleted successfully!";
            }
            return RedirectToAction("Index");
        }

        public IActionResult IndexSemester()
        {
            var semesters = _db.Semesters.ToList();
            return View(semesters);
        }

        // GET: Semester/Create
        public IActionResult CreateSemester()
        {
            return View();
        }

        // POST: Semester/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateSemester(Semester model)
        {
            if (ModelState.IsValid)
            {
                _db.Semesters.Add(model);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Semester created successfully!";
                return RedirectToAction("IndexSemester");
            }
            return View(model);
        }

        // GET: Semester/Edit/{id}
        public IActionResult EditSemester(int id)
        {
            var semester = _db.Semesters.Find(id);
            if (semester == null)
            {
                return NotFound();
            }
            return View(semester);
        }

        // POST: Semester/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditSemester(Semester model)
        {
            if (ModelState.IsValid)
            {
                _db.Semesters.Update(model);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Semester updated successfully!";
                return RedirectToAction("IndexSemester");
            }
            return View(model);
        }

        // GET: Semester/Delete/{id}
        public IActionResult DeleteSemester(int id)
        {
            var semester = _db.Semesters.Find(id);
            if (semester == null)
            {
                return NotFound();
            }
            return View(semester);
        }

        // POST: Semester/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmedSemester(int id)
        {
            var semester = _db.Semesters.Find(id);
            if (semester != null)
            {
                _db.Semesters.Remove(semester);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Semester deleted successfully!";
            }
            return RedirectToAction("IndexSemester");
        }

        public IActionResult IndexSubject()
        {
            var subjects = _db.Subjects.Include(sj => sj.SubjectCategory).ToList();
            return View(subjects);
        }

        // GET: Subject/Create
        public IActionResult CreateSubject()
        {
            ViewBag.SubjectCategories = new SelectList(_db.SubjectCategories, "SubjectCategoryId", "CategoryName");
            return View();
        }

        //// POST: Subject/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult CreateSubject(Subject model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.Subjects.Add(model);
        //        try
        //        {
        //            _db.SaveChanges();
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine($"Error: {ex.Message}");
        //        }
        //        TempData["SuccessMessage"] = "Subject created successfully!";
        //        return RedirectToAction("IndexSubject");
        //    }
        //    ViewBag.SubjectCategories = new SelectList(_db.SubjectCategories, "SubjectCategoryId", "CategoryName");
        //    return View(model);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateSubject(Subject model)
        {
            if (!ModelState.IsValid)
            {
                Console.WriteLine("Validation Errors:");
                foreach (var state in ModelState)
                {
                    if (state.Value.Errors.Count > 0)
                    {
                        Console.WriteLine($"Key: {state.Key}, Error: {string.Join(", ", state.Value.Errors.Select(e => e.ErrorMessage))}");
                    }
                }

                ViewBag.SubjectCategories = new SelectList(_db.SubjectCategories, "SubjectCategoryId", "CategoryName");
                return View(model);
            }

            Console.WriteLine($"SubjectCategoryId: {model.SubjectCategoryId}");

            try
            {
                _db.Subjects.Add(model);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Subject created successfully!";
                return RedirectToAction("IndexSubject");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ViewBag.SubjectCategories = new SelectList(_db.SubjectCategories, "SubjectCategoryId", "CategoryName");
                return View(model);
            }
        }



        // GET: Subject/Edit/{id}
        public IActionResult EditSubject(int id)
        {
            var subject = _db.Subjects.Find(id);
            if (subject == null)
            {
                return NotFound();
            }
            ViewBag.SubjectCategories = new SelectList(_db.SubjectCategories, "SubjectCategoryId", "CategoryName", subject.SubjectCategoryId);
            return View(subject);
        }

        // POST: Subject/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditSubject(Subject model)
        {
            if (ModelState.IsValid)
            {
                _db.Subjects.Update(model);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Subject updated successfully!";
                return RedirectToAction("IndexSubject");
            }
            ViewBag.SubjectCategories = new SelectList(_db.SubjectCategories, "SubjectCategoryId", "CategoryName", model.SubjectCategoryId);
            return View(model);
        }

        // GET: Subject/Delete/{id}
        public IActionResult DeleteSubject(int id)
        {
            var subject = _db.Subjects.Find(id);
            if (subject == null)
            {
                return NotFound();
            }
            return View(subject);
        }

        // POST: Subject/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmedSubject(int id)
        {
            var subject = _db.Subjects.Find(id);
            if (subject != null)
            {
                _db.Subjects.Remove(subject);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Subject deleted successfully!";
            }
            return RedirectToAction("IndexSubject");
        }

        public IActionResult IndexCategory()
        {
            var categories = _db.SubjectCategories.ToList();
            return View(categories);
        }

        // GET: SubjectCategory/Create
        public IActionResult CreateCategory()
        {
            return View();
        }

        // POST: SubjectCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCategory(SubjectCategory model)
        {
            if (ModelState.IsValid)
            {
                _db.SubjectCategories.Add(model);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Subject category created successfully!";
                return RedirectToAction("IndexCategory");
            }
            return View(model);
        }

        // GET: SubjectCategory/Edit/{id}
        public IActionResult EditCategory(int id)
        {
            var category = _db.SubjectCategories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: SubjectCategory/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCategory(SubjectCategory model)
        {
            if (ModelState.IsValid)
            {
                _db.SubjectCategories.Update(model);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Subject category updated successfully!";
                return RedirectToAction("IndexCategory");
            }
            return View(model);
        }

        // GET: SubjectCategory/Delete/{id}
        public IActionResult DeleteCategory(int id)
        {
            var category = _db.SubjectCategories.Find(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: SubjectCategory/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmedCategory(int id)
        {
            var category = _db.SubjectCategories.Find(id);
            if (category != null)
            {
                _db.SubjectCategories.Remove(category);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Subject category deleted successfully!";
            }
            return RedirectToAction("IndexCategory");
        }
        public IActionResult IndexSchedule()
        {
            var schedules = _db.ClassSchedules
                .Include(cs => cs.ClassManagement)
                .ToList();
            return View(schedules);
        }

        //public async Task<IActionResult> IndexManage()
        //{
        //    var classManagements = await _db.ClassManagements
        //        .Include(cm => cm.Class)
        //        .Include(cm => cm.Teacher)
        //        .Include(cm => cm.Course)
        //        .Include(cm => cm.Semester)
        //        .Include(cm => cm.Subject)
        //        .ToListAsync();
        //    return View(classManagements);
        //}

        // GET: ClassSchedule/Create
        public IActionResult CreateSchedule()
        {
            ViewBag.ClassManagements = new SelectList(_db.ClassManagements, "CM_ID", "CM_ID");
            return View();
        }

        // POST: ClassSchedule/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateSchedule(ClassSchedule model)
        {
            if (ModelState.IsValid)
            {
                _db.ClassSchedules.Add(model);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Class schedule created successfully!";
                return RedirectToAction("IndexSchedule");
            }
            ViewBag.ClassManagements = new SelectList(_db.ClassManagements, "CM_ID", "CM_ID");
            return View(model);
        }

        // GET: ClassSchedule/Edit/{id}
        public IActionResult EditSchedule(int id)
        {
            var schedule = _db.ClassSchedules.Find(id);
            if (schedule == null)
            {
                return NotFound();
            }
            ViewBag.ClassManagements = new SelectList(_db.ClassManagements, "CM_ID", "CM_ID", schedule.CM_ID);
            return View(schedule);
        }

        // POST: ClassSchedule/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditSchedule(ClassSchedule model)
        {
            if (ModelState.IsValid)
            {
                _db.ClassSchedules.Update(model);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Class schedule updated successfully!";
                return RedirectToAction("IndexSchedule");
            }
            ViewBag.ClassManagements = new SelectList(_db.ClassManagements, "CM_ID", "CM_ID", model.CM_ID);
            return View(model);
        }

        // GET: ClassSchedule/Delete/{id}
        public IActionResult DeleteSchedule(int id)
        {
            var schedule = _db.ClassSchedules.Find(id);
            if (schedule == null)
            {
                return NotFound();
            }
            return View(schedule);
        }

        // POST: ClassSchedule/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmedSchedule(int id)
        {
            var schedule = _db.ClassSchedules.Find(id);
            if (schedule != null)
            {
                _db.ClassSchedules.Remove(schedule);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Class schedule deleted successfully!";
            }
            return RedirectToAction("IndexSchedule");
        }
    }
}
