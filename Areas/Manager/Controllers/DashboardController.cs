using Microsoft.AspNetCore.Mvc;
using ST10287116_PROG6212_POE_P2.Models;
using ST10287116_PROG6212_POE_P2.Services;

namespace ST10287116_PROG6212_POE_P2.Areas.Manager.Controllers
{
    [Area("Manager")]
    public class DashboardController(ClaimService claimService) : Controller
    {
        private readonly ClaimService _claimService = claimService;

        public IActionResult Index()
        {
            var claims = _claimService.GetVerifiedClaims();  // NEW: Filter Verified (implement in service: _context.Claims.Where(c => c.Status == ClaimStatus.Verified).Include(c => c.Documents).ToList();)
            return View(claims);
        }

        [HttpPost]
        public IActionResult Approve(int id)
        {
            _claimService.UpdateStatus(id, ClaimStatus.Approved);
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
