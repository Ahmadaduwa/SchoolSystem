using Microsoft.AspNetCore.Mvc;
using SchoolSystem.Data;

namespace SchoolSystem.Controllers
{
    public class ActivityManagementController : Controller
    {
        private readonly AppDbContext _db;

        public ActivityManagementController(AppDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
