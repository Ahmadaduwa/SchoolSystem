using Microsoft.AspNetCore.Mvc;
using SchoolSystem.Data;
using SchoolSystem.Models;

namespace SchoolSystem.Controllers
{
    public class TeacherController : Controller
    {
        private readonly AppDbContext _db;

        public TeacherController(AppDbContext db)
        {
            _db = db;
        }

        // GET: /Teacher/Index
        public IActionResult Index()
        {
            // Fetch all courses from the database
            IEnumerable<Course> allCourses = _db.Courses;
            return View(allCourses); // Make sure Index.cshtml exists
        }

        // GET: /Teacher/Create
        public IActionResult Create()
        {
            return View(); // Render the Create form
        }

        // POST: /Teacher/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Course obj)
        {
            if (ModelState.IsValid)
            {
                // Save the course
                _db.Courses.Add(obj);
                _db.SaveChanges();
                TempData["SuccessMessage"] = "Course created successfully!";
                return RedirectToAction("Index");
            }
            return View(obj);
        }

        // GET: /Teacher/EditByCourseCode
        public IActionResult CourseCode()
        {
            return View(); // Render the search form
        }

        // POST: /Teacher/EditByCourseCode
        [HttpPost]
        public IActionResult CourseCode(string CourseCode)
        {
            // Search for the course by CourseCode
            var obj = _db.Courses.FirstOrDefault(c => c.CourseCode == CourseCode);

            if (obj == null)
            {
                TempData["ErrorMessage"] = "No course found with the given code.";
                return RedirectToAction("Index");
            }

            // Redirect to the Edit view with the CourseID
            return RedirectToAction("Edit", new { id = obj.CourseID });
        }

        [HttpGet]
        [Route("Teacher/Edit")] // This route will map to this method
        public IActionResult Edit()
        {
            return View(); // Ensure Create.cshtml exists
        }

        // GET: /Teacher/Edit/{id}
        [HttpGet]
        [Route("Teacher/Edit/{id:int}")]
        public IActionResult Edit(int id)
        {
            var course = _db.Courses.Find(id); // Retrieve course by ID
            if (course == null)
            {
                return NotFound(); // Handle not found case
            }
            return View(course); // Pass course data to the view
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Course obj)
        {
            if (!ModelState.IsValid)
            {
                return View(obj); // Re-render the form if validation fails
            }

            var courseToUpdate = _db.Courses.Find(obj.CourseID); // Find the course by ID
            if (courseToUpdate != null)
            {
                // Update course properties
                courseToUpdate.CourseCode = obj.CourseCode;
                courseToUpdate.CourseName = obj.CourseName;
                courseToUpdate.LearningObjective = obj.LearningObjective;
                courseToUpdate.Units = obj.Units;
                courseToUpdate.Category = obj.Category;
                courseToUpdate.Prerequisites = obj.Prerequisites;
                courseToUpdate.Postrequisites = obj.Postrequisites;

                _db.SaveChanges(); // Save changes to the database
                TempData["SuccessMessage"] = "Course updated successfully!";
                return RedirectToAction("Index"); // Redirect to the course list
            }

            TempData["ErrorMessage"] = "Course not found.";
            return View(obj);
        }



        public IActionResult EditById()
        {
            return View(); // Render the search form
        }

        // POST: /New/EditById (Search student by ID)
        [HttpPost]
        public IActionResult EditById(int id)
        {
            var obj = _db.Courses.Find(id);

            if (obj == null)
            {
                TempData["ErrorMessage"] = "No course found with the given ID.";
                return RedirectToAction("EditById");
            }

            return RedirectToAction("Edit", new { id = obj.CourseID }); // Redirect to the Edit form with the student ID
        }
        // GET: /Teacher/EditByCourseCode


        // GET: /Teacher/Delete/{id}
        public IActionResult Delete(int id)
        {
            var obj = _db.Courses.Find(id); // Find the course by ID
            if (obj == null)
            {
                return NotFound(); // Return 404 if not found
            }
            return View(obj); // Render the Delete confirmation page
        }

        // POST: /Teacher/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var obj = _db.Courses.Find(id); // Find the course by ID
            if (obj != null)
            {
                _db.Courses.Remove(obj); // Remove the course
                _db.SaveChanges(); // Save changes to the database
                TempData["SuccessMessage"] = "Course deleted successfully!";
                return RedirectToAction("Index");
            }
            TempData["ErrorMessage"] = "Course not found.";
            return RedirectToAction("Index");
        }

        // GET: /Teacher/Details/{id}
        public IActionResult Details(int id)
        {
            var obj = _db.Courses.Find(id); // Find the course by ID
            if (obj == null)
            {
                return NotFound(); // Return 404 if not found
            }
            return View(obj); // Render the Details view
        }

        public IActionResult SearchPostrequisites()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SearchPrerequisiteById(int id)
        {
            var course = _db.Courses.FirstOrDefault(c => c.CourseID == id);
            if (course == null)
            {
                return Json(new { success = false });
            }

            return Json(new
            {
                success = true,
                courseName = course.CourseName
            });
        }
        public IActionResult Cancel(int id, string reason)
        {
            // Find the course to cancel
            var course = _db.Courses.Find(id);

            if (course == null)
            {
                return NotFound(); // Return 404 if course is not found
            }

            // Create a new CanceledCourse object
            var canceledCourse = new CanceledCourse
            {
                CourseID = course.CourseID,
                CourseCode = course.CourseCode,
                CourseName = course.CourseName,
                LearningObjective = course.LearningObjective,
                Units = course.Units,
                Category = course.Category,
                Prerequisites = course.Prerequisites,
                Postrequisites = course.Postrequisites,
                CanceledDate = DateTime.Now,
                Reason = reason
            };

            // Add the canceled course to the CanceledCourses table
            _db.CanceledCourses.Add(canceledCourse);

            // Remove the course from the Courses table
            _db.Courses.Remove(course);

            // Save changes to the database
            _db.SaveChanges();

            TempData["SuccessMessage"] = "Course canceled successfully!";
            return RedirectToAction("Index");
        }
        public IActionResult CanceledCourses()
        {
            var canceledCourses = _db.CanceledCourses.ToList(); // Fetch all canceled courses
            return View(canceledCourses); // Pass the data to the view
        }

    }
}
