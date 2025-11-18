using ST10287116_PROG6212_POE_P2.Models;
using Microsoft.EntityFrameworkCore;

namespace ST10287116_PROG6212_POE_P2.Services
{
    public class ClaimService
    {
        private readonly ApplicationDbContext _context;

        public ClaimService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Claim> GetUserClaims(string userId)
        {
            return _context.Claims.Where(c => c.UserId == userId).ToList();
        }

        public IEnumerable<Claim> GetAllClaims(string? search = "")
        {
            var query = _context.Claims.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.ClaimId.Contains(search) || c.Status.ToString().Contains(search));
            }
            return query.ToList();
        }

        // Updated: include Documents
        public IEnumerable<Claim> GetPendingClaims(string? search = "")
        {
            var query = _context.Claims
                .Where(c => c.Status == ClaimStatus.Pending)
                .Include(c => c.Documents)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(c => c.ClaimId.Contains(search));
            }
            return query.ToList();
        }

        // Convenience overload without search (matches your one-line version)
        public IEnumerable<Claim> GetPendingClaims() =>
            _context.Claims
                .Where(c => c.Status == ClaimStatus.Pending)
                .Include(c => c.Documents)
                .ToList();

        // Already matching your desired implementation
        public IEnumerable<Claim> GetVerifiedClaims() =>
            _context.Claims
                .Where(c => c.Status == ClaimStatus.Verified)
                .Include(c => c.Documents)
                .ToList();

        public void UpdateStatus(int id, ClaimStatus status)
        {
            var claim = _context.Claims.Find(id);
            if (claim != null)
            {
                claim.Status = status;
                claim.LastUpdated = DateTime.Now;
                _context.SaveChanges();
            }
        }

        public void CreateClaim(Claim claim)
        {
            _context.Claims.Add(claim);
            if (claim.Documents != null && claim.Documents.Count > 0)
            {
                _context.Set<Document>().AddRange(claim.Documents);
            }
            _context.SaveChanges();
        }
    }
}