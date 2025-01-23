using Microsoft.AspNetCore.Mvc;

namespace SchoolSystem.Controllers
{
    public class AcademicController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
