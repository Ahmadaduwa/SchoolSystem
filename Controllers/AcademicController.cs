using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SchoolSystem.Controllers
{
    [Authorize(Policy = "AcademicPolicy")]
    public class AcademicController : Controller
    {

        [Route("Course")]
        public IActionResult CourseManagement()
        {
            return View();
        }
    }
}
