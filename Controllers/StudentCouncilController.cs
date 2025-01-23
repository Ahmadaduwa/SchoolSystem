using Microsoft.AspNetCore.Mvc;

namespace SchoolSystem.Controllers
{
    public class StudentCouncilController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
