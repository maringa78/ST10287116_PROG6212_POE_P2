using Microsoft.AspNetCore.Mvc; 
using Microsoft.AspNetCore.Diagnostics;
using ST10287116_PROG6212_POE_P2.Models;
using ST10287116_PROG6212_POE_P2.Services;  

namespace ST10287116_PROG6212_POE_P2.Controllers
{
    public class AccountController(AuthService authService, ILogger<AccountController> logger) : Controller 
    {
        private readonly AuthService _authService = authService;
        private readonly ILogger<AccountController> _logger = logger;

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(UserLogin model)
        {
            if (ModelState.IsValid)
            {
                var user = _authService.ValidateUser(model.Email, model.Password);
                if (user != null)
                {
                    // Set session data (MUST for access control)
                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("Role", user.Role.ToString());
                    HttpContext.Session.SetString("NameSurname", user.Username);  // Use Username as fallback
                    HttpContext.Session.SetString("HourlyRate", user.HourlyRate.ToString("F2"));  // For lecturer auto-calc

                    // NEW: Role-based redirect (checklist: prevent unauthorized access)
                    var role = user.Role.ToString().ToLower();
                    return role switch
                    {
                        "hr" => RedirectToAction("Index", "User", new { area = "HR" }),  // HR to user management
                        "lecturer" => RedirectToAction("Index", "Dashboard", new { area = "Lecturer" }),  // Lecturer to submission
                        "coordinator" => RedirectToAction("Index", "Track", new { area = "Coordinator" }),  // Coordinator to verify
                        "manager" => RedirectToAction("Index", "Dashboard", new { area = "Manager" }),  // Manager to approve
                        _ => RedirectToAction("Index", "Home")  // Fallback
                    };
                }
                ModelState.AddModelError("", "Invalid credentials");
            }
            return View(model);
            
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
