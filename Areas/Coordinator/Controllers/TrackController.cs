using ST10287116_PROG6212_POE_P2.Services;
using Microsoft.AspNetCore.Mvc;
using ST10287116_PROG6212_POE_P2.Models;

namespace ST10287116_PROG6212_POE_P2.Areas.Coordinator.Controllers
{
    [Area("Coordinator")]
    public class TrackController(ClaimService claimService) : Controller
    {
        private readonly ClaimService _claimService = claimService;

        public IActionResult Index(string? search)
        {
            var claims = _claimService.GetAllClaims(search);
            return View(claims);
        }
    }
}
