using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SchoolSystem.Models;
using SchoolSystem.Models.ViewModels;

namespace SchoolSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<Users> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<Users> signInManager,
            IConfiguration configuration)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [Route("")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        [Route("Home")]
        public IActionResult Home()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            TempData["ErrorMessage"] = null;
            return View();
        }

        [HttpPost("login")]
        [Route("login")]
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

        [HttpPost("register")]
        [Route("register")]
        public async Task<IActionResult> Register([FromForm] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fill in all required fields correctly.";
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
                FirstName = model.FirstName,
                LastName = model.LastName
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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
