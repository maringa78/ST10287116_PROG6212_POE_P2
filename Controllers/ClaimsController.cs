using Microsoft.AspNetCore.Mvc;
using ST10287116_PROG6212_POE_P2.Services;
using ST10287116_PROG6212_POE_P2.Models;
using Microsoft.AspNetCore.Http;

namespace ST10287116_PROG6212_POE_P2.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly ClaimService _claimService;

        public ClaimsController(ClaimService claimService)
        {
            _claimService = claimService;
        }

        [HttpGet]
        public IActionResult Submit()
        {
            return View(new Claim());  // Pass empty model for binding
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(Claim model, List<IFormFile> files)  // NEW: Files param for uploads
        {
            if (!ModelState.IsValid)
            {
                return View(model);  // Return with errors (e.g., invalid hours/rate)
            }

            model.ClaimDate = model.ClaimDate == default ? DateTime.Now : model.ClaimDate;
            model.Status = ClaimStatus.Pending;  // Initial status for workflow (coordinator verifies next)
            var userId = HttpContext.Session.GetString("UserId");
            model.UserId = string.IsNullOrEmpty(userId) ? "1" : userId;
            model.LecturerId = 1;  // Ensure lecturer filter
            model.Created = DateTime.Now;
            model.LastUpdated = DateTime.Now;
            model.ClaimMonth = model.ClaimDate.ToString("MMMM yyyy");  // NEW: Auto-set e.g., "November 2025"

            // NEW: Calculate Total Amount (Hours * Rate, fallback to Amount)
            if (model.HoursWorked > 0 && model.HourlyRate > 0)
            {
                model.TotalAmount = model.HoursWorked * model.HourlyRate;
            }
            else
            {
                model.TotalAmount = model.Amount;
            }

            // NEW: Handle supporting documents upload
            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    // Validate size <5MB and type (PDF or image)
                    if (file.Length > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("", "File too large (max 5MB).");
                        return View(model);
                    }
                    if (file.ContentType != "application/pdf" && !file.ContentType.StartsWith("image/"))
                    {
                        ModelState.AddModelError("", "Invalid file type (PDF or image only).");
                        return View(model);
                    }

                    // Save file with unique name
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(uploadsDir, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Add to Documents list for viewing in admin
                    model.Documents.Add(new Document
                    {
                        FileName = file.FileName,
                        FilePath = $"/uploads/{uniqueFileName}"
                    });
                }
            }

            _claimService.CreateClaim(model);  // Save claim + documents

            return RedirectToAction("Index", "Dashboard", new { area = "Lecturer" });
        }
    }
}