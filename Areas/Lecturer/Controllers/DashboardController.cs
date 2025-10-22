using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ST10287116_PROG6212_POE_P2.Models;

namespace ST10287116_PROG6212_POE_P2.Areas.Lecturer.Controllers
{
    [Area("Lecturer")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Prefer showing claims for the signed-in user; fall back to lecturerId if no session user.
            string? userId = HttpContext.Session.GetString("UserId");
            int lecturerId = 1;

            var query = _context.Claims.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(c => c.UserId == userId || c.LecturerId == lecturerId);
            }
            else
            {
                query = query.Where(c => c.LecturerId == lecturerId);
            }

            var claims = query
                .OrderByDescending(c => c.ClaimDate)
                .ToList();

            return View(claims);
        }
    }
}
