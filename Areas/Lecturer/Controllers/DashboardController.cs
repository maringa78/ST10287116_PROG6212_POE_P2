using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using ST10287116_PROG6212_POE_P2.Services;

namespace ST10287116_PROG6212_POE_P2.Areas.Lecturer.Controllers
{
    [Area("Lecturer")]
    [Authorize(Roles = "Lecturer")]
    public class DashboardController : Controller
    {
        private readonly ClaimService _claimService;
        public DashboardController(ClaimService claimService) => _claimService = claimService;

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var claims = _claimService.GetForUser(userId);
            return View(claims);
        }
    }
}
