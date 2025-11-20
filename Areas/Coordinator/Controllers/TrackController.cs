using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ST10287116_PROG6212_POE_P2.Services;
using ST10287116_PROG6212_POE_P2.Models;

namespace ST10287116_PROG6212_POE_P2.Areas.Coordinator.Controllers
{
    [Area("Coordinator")]
    [Authorize(Roles = "Coordinator")]
    public class TrackController : Controller
    {
        private readonly ClaimService _claims;
        public TrackController(ClaimService claims) => _claims = claims;

        public IActionResult Index()
        {
            var pending = _claims.GetPendingForCoordinator();
            return View(pending);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Verify(int id)
        {
            _claims.UpdateStatus(id, ClaimStatus.Verified);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id)
        {
            _claims.UpdateStatus(id, ClaimStatus.Rejected);
            return RedirectToAction(nameof(Index));
        }
    }
}
