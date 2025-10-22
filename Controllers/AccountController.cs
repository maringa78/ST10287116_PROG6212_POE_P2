using Microsoft.AspNetCore.Mvc; 
using ST10287116_PROG6212_POE_P2.Models;
using ST10287116_PROG6212_POE_P2.Services;  
namespace ST10287116_PROG6212_POE_P2.Controllers
{
    public class AccountController(AuthService authService) : Controller 
    {
        private readonly AuthService _authService = authService;

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
                    HttpContext.Session.SetString("UserId", user.Id.ToString());
                    HttpContext.Session.SetString("Role", user.Role.ToString());
                    return RedirectToAction("Index", user.Role.ToString() == "Lecturer" ? "Dashboard" :
                                             user.Role.ToString() == "Coordinator" ? "Track" : "Dashboard",
                                             new { area = user.Role.ToString() });
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
    public class UserLogin
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
