using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ST10287116_PROG6212_POE_P2.Services;
using ST10287116_PROG6212_POE_P2.Models;

namespace ST10287116_PROG6212_POE_P2.Areas.Coordinator.Controllers
{
    [Area("Coordinator")]
    [Authorize(Roles = "Coordinator")]
    public class WorkflowController : Controller
    {
        private readonly ClaimService _claimService;
        public WorkflowController(ClaimService claimService) => _claimService = claimService;

        public IActionResult Pending() =>
            View(_claimService.GetPendingClaims());

        [HttpPost]
        public IActionResult Verify(int id)
        {
            _claimService.UpdateStatus(id, ClaimStatus.Verified);
            return RedirectToAction("Pending");
        }
    }
}