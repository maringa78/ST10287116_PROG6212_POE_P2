using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ST10287116_PROG6212_POE_P2.Services;
using ST10287116_PROG6212_POE_P2.Models;
using Microsoft.AspNetCore.Http;
using ST10287116_PROG6212_POE_P2.Services.Validation;
using ST10287116_PROG6212_POE_P2.Data;
using System.Security.Claims;
using ClaimModel = ST10287116_PROG6212_POE_P2.Models.Claim;

namespace ST10287116_PROG6212_POE_P2.Controllers
{
    [Authorize(Roles = "Lecturer")]
    public class ClaimsController : Controller
    {
        private readonly ClaimService _service;
        private readonly ILogger<ClaimsController> _log;
        private readonly ApplicationDbContext _ctx;
        public ClaimsController(ClaimService service, ILogger<ClaimsController> log, ApplicationDbContext ctx)
        {
            _service = service;
            _log = log;
            _ctx = ctx;
        }

        public IActionResult Submit()
        {
            var sid = HttpContext.Session.GetString("UserId");
            var lecturer = !string.IsNullOrEmpty(sid)
                ? _ctx.Users.FirstOrDefault(u => u.Id.ToString() == sid)
                : null;
            ViewBag.HourlyRate = lecturer?.HourlyRate ?? 0m;
            return View(new ClaimModel { HourlyRate = lecturer?.HourlyRate ?? 0m });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(ClaimModel model, List<IFormFile> files)
        {
            var uid = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(uid))
                return RedirectToAction("Login", "Account");

            var lecturerEntity = _ctx.Users.FirstOrDefault(u => u.Id.ToString() == uid);
            if (lecturerEntity == null)
            {
                ModelState.AddModelError("", "Lecturer not found.");
                return View(model);
            }

            if (model.HourlyRate <= 0)
                model.HourlyRate = lecturerEntity.HourlyRate;

            if (!ModelState.IsValid)
            {
                _log.LogWarning("Model invalid on submit for user {UserId}. Errors: {Errors}",
                    uid,
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                ViewBag.HourlyRate = lecturerEntity.HourlyRate;
                return View(model);
            }

            model.ClaimId = "CLM-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"); // unique
            model.ClaimDate = model.ClaimDate == default ? DateTime.Now : model.ClaimDate;
            model.Status = ClaimStatus.Pending;
            model.UserId = uid;
            model.LecturerId = lecturerEntity.Id;
            model.Created = DateTime.Now;
            model.LastUpdated = DateTime.Now;
            model.ClaimMonth = model.ClaimDate.ToString("MMMM yyyy");

            var monthlyHours = _service.GetMonthlyHoursForUser(lecturerEntity.Id, DateTime.Now.Year, DateTime.Now.Month);
            if (monthlyHours + model.HoursWorked > 180)
            {
                ModelState.AddModelError("", "Monthly hour limit (180) exceeded.");
                ViewBag.HourlyRate = lecturerEntity.HourlyRate;
                return View(model);
            }

            model.TotalAmount = (model.HoursWorked > 0 && model.HourlyRate > 0)
                ? model.HoursWorked * model.HourlyRate
                : model.Amount;

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            foreach (var file in files)
            {
                if (file.Length <= 0) continue;
                if (file.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "File too large (max 5MB).");
                    ViewBag.HourlyRate = lecturerEntity.HourlyRate;
                    return View(model);
                }
                if (file.ContentType != "application/pdf" && !file.ContentType.StartsWith("image/"))
                {
                    ModelState.AddModelError("", "Invalid file type (PDF or image only).");
                    ViewBag.HourlyRate = lecturerEntity.HourlyRate;
                    return View(model);
                }
                var uniqueFileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(uploadsDir, uniqueFileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                model.Documents.Add(new Document
                {
                    FileName = file.FileName,
                    FilePath = $"/uploads/{uniqueFileName}"
                });
            }

            var (ok, error) = ClaimValidator.ValidateBusiness(model, monthlyHours);
            if (!ok)
            {
                ModelState.AddModelError("", error!);
                ViewBag.HourlyRate = lecturerEntity.HourlyRate;
                return View(model);
            }

            _log.LogInformation("Saving claim for UserId={UserId} ClaimId={ClaimId}", uid, model.ClaimId);
            _service.Create(model);

            return RedirectToAction("Status");
        }

        public IActionResult Status()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            var mine = _service.GetUserClaims(userId);
            return View(mine);
        }

        [HttpGet]
        public IActionResult DebugAll()
        {
            var all = _service.GetForUser(HttpContext.Session.GetString("UserId"));
            return Json(all.Select(c => new { c.Id, c.ClaimId, c.UserId, c.Type, c.TotalAmount }));
        }
    }
}