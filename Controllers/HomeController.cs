using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SchoolSystem.Data;
using SchoolSystem.Models;
using SchoolSystem.Models.UserManagement;
using SchoolSystem.Models.ViewModels;
using SchoolSystem.Models.Alert;

namespace SchoolSystem.Controllers
{   
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _db;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<Users> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<Users> signInManager,
            AppDbContext db,
            IConfiguration configuration)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _db = db;
        }

        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("NotReady")]
        public IActionResult NotReady()
        {
            return View();
        }

        [Route("NotAuthorized")]
        public IActionResult NotAuthorized()
        {
            return View();
        }

        [Authorize]
        [Route("Home")]
        public async Task<IActionResult> Home()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var notifications = await _db.Notifications
                .Include(n => n.Profile)
                .Where(n => n.Profile.UserId == user.Id)
                .OrderByDescending(n => n.NotificationTime)
                .ToListAsync();

            return View(notifications);
        }

        [HttpGet]
        [Route("login")]
        public IActionResult Login()
        {
            TempData["ErrorMessage"] = null;
            return View();
        }

        [HttpPost]
        [Route("login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill in all required fields.";
                return View(model);
            }
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill in all required fields.";
                return View(model);
            }

            try
            {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
                {
                    var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.NameIdentifier, user.Id!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

                    var userRoles = await _userManager.GetRolesAsync(user);
                    authClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

                    var jwtKey = _configuration["Jwt:Key"];
                    if (string.IsNullOrEmpty(jwtKey))
                    {
                        throw new InvalidOperationException("JWT key is not configured.");
                    }

                    var token = new JwtSecurityToken(
                        issuer: _configuration["Jwt:Issuer"],
                        expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"]!)),
                        claims: authClaims,
                        signingCredentials: new SigningCredentials(
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                            SecurityAlgorithms.HmacSha256
                        )
                    );

                    HttpContext.Response.Cookies.Append("AuthToken", new JwtSecurityTokenHandler().WriteToken(token), new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddMinutes(double.Parse(_configuration["Jwt:ExpiryMinutes"]!))
                    });

                    return RedirectToAction("Home", "Home");
                }

                TempData["ErrorMessage"] = "Invalid login attempt.";
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login.");
                TempData["ErrorMessage"] = "An error occurred. Please try again later.";
                return View(model); ;
            }
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Response.Cookies.Delete("AuthToken");

            return RedirectToAction("Index", "Home");
        }
        /*
        [HttpGet]
        [Route("register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [Route("register")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([FromForm] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill in all required fields correctly.";
                return View(model);
            }

            // Check if the email or username is already in use
            if (string.IsNullOrEmpty(model.Email))
            {
                TempData["ErrorMessage"] = "Email is required.";
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Username))
            {
                TempData["ErrorMessage"] = "Username is required.";
                return View(model);
            }

            if (string.IsNullOrEmpty(model.FirstName))
            {
                TempData["ErrorMessage"] = "FirstName is required.";
                return View(model);
            }

            if(string.IsNullOrEmpty(model.LastName))
            {
                TempData["ErrorMessage"] = "LastName is required.";
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Password))
            {
                TempData["ErrorMessage"] = "Password is required.";
                return View(model);
            }

            // Check if the email or username is already in use
            var existingUser = await _userManager.FindByEmailAsync(model.Email)
                               ?? await _userManager.FindByNameAsync(model.Username);

            if (existingUser != null)
            {
                TempData["ErrorMessage"] = "The email or username is already in use.";
                return View(model);
            }

            var user = new Users
            {
                UserName = model.Username,
                Email = model.Email,
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Assign default role (if required)
                if (!await _roleManager.RoleExistsAsync("User"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("User"));
                }

                await _userManager.AddToRoleAsync(user, "User");

                TempData["SuccessMessage"] = "Registration successful!";
                return View(model);
            }

            // Add errors to ModelState to display in the view
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            TempData["ErrorMessage"] = "Registration failed. Please try again.";
            return View(model);
        }
        }
        */



        [HttpGet]
        [Route("/Teacher/Schedule")]
        [Authorize(Policy = "TeacherPolicy")]
        public async Task<IActionResult> Schedule()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var teacher = await _db.Users
                         .Include(u => u.Profile!).ThenInclude(p => p.Teacher!)
                         .FirstOrDefaultAsync(u => u.Id == userId);
            if (teacher?.Profile?.Teacher == null)
                return RedirectToAction("Index", "Home");

            int teacherId = teacher.Profile.Teacher.TeacherId;
            var raw = await _db.ClassManagements
                        .AsNoTracking()
                        .Include(cm => cm.Course)
                        .Include(cm => cm.Class).ThenInclude(c => c.GradeLevels)
                        .Include(cm => cm.Semester)
                        .Include(cm => cm.ClassSchedules)
                        .Where(cm => cm.TeacherId == teacherId)
                        .SelectMany(cm => cm.ClassSchedules.Select(cs => new TeachingScheduleViewModel
                        {
                            CourseName = cm.Course.CourseName,
                            GradeLevel = cm.Class.GradeLevels.Name!,
                            ClassNumber = cm.Class.ClassNumber,
                            DayOfWeekEn = cs.DayOfWeek, // English key
                            StartTime = cs.StartTime,
                            EndTime = cs.EndTime,
                            SemesterYear = cm.Semester.SemesterYear,
                            SemesterNumber = cm.Semester.SemesterNumber
                        }))
                        .ToListAsync();

            var vm = new TeachingSchedulePageViewModel
            {
                SemesterNumber = raw.FirstOrDefault()?.SemesterNumber ?? 0,
                SemesterYear = raw.FirstOrDefault()?.SemesterYear ?? "-"
            };

            // สร้าง Matrix โดยใช้ key เป็น English
            foreach (var day in vm.Days)
            {
                vm.Matrix[day.English] = new Dictionary<string, List<TeachingScheduleViewModel>>();
                foreach (var p in vm.Periods)
                {
                    vm.Matrix[day.English][p.Name] = raw
                        .Where(s => s.DayOfWeekEn == day.English
                                    && s.StartTime < p.End && s.EndTime > p.Start)
                        .ToList();
                }
            }

            return View(vm);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Add function here


    }
}
