using ST10287116_PROG6212_POE_P2.Services;
using Microsoft.AspNetCore.Mvc;
using ST10287116_PROG6212_POE_P2.Models;

namespace ST10287116_PROG6212_POE_P2.Areas.Coordinator.Controllers
{
    [Area("Coordinator")]
    public class TrackController(ClaimService claimService) : Controller
    {
        private readonly ClaimService _claimService = claimService;
        public IActionResult Index()
        {
            var claims = _claimService.GetPendingClaims();  // Filter Pending
            return View(claims);
        }

        [HttpPost]
        public IActionResult Verify(int id)
        {
            _claimService.UpdateStatus(id, ClaimStatus.Verified);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Reject(int id)
        {
            _claimService.UpdateStatus(id, ClaimStatus.Rejected);
            return RedirectToAction("Index");
        }
    }
}
