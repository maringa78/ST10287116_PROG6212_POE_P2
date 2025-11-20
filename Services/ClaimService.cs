using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ST10287116_PROG6212_POE_P2.Data;
using ST10287116_PROG6212_POE_P2.Models;

namespace ST10287116_PROG6212_POE_P2.Services
{
    public class ClaimService
    {
        private readonly ApplicationDbContext _ctx;
        public ClaimService(ApplicationDbContext ctx) => _ctx = ctx;

        public void Create(Claim claim)
        {
            _ctx.Claims.Add(claim);
            _ctx.SaveChanges();
        }

        public IEnumerable<Claim> GetForUser(string? userId) =>
            !string.IsNullOrEmpty(userId)
                ? _ctx.Claims.Where(c => c.UserId == userId).Include(c => c.Documents).ToList()
                : Enumerable.Empty<Claim>();

        public IEnumerable<Claim> GetPendingForCoordinator() =>
            _ctx.Claims.Where(c => c.Status == ClaimStatus.Pending).Include(c => c.Documents).ToList();

        public IEnumerable<Claim> GetPendingClaims() =>
            _ctx.Claims.Where(c => c.Status == ClaimStatus.Pending)
                       .Include(c => c.Documents)
                       .ToList();

        public int GetMonthlyHoursForUser(int lecturerId, int year, int month) =>
            _ctx.Claims
                .Where(c => c.LecturerId == lecturerId
                            && c.ClaimDate.Year == year
                            && c.ClaimDate.Month == month)
                .Sum(c => c.HoursWorked);

        public IEnumerable<Claim> GetVerifiedClaims() =>
            _ctx.Claims
                .Where(c => c.Status == ClaimStatus.Verified)
                .Include(c => c.Documents)
                .ToList();

        public void UpdateStatus(int claimId, ClaimStatus newStatus)
        {
            var claim = _ctx.Claims.FirstOrDefault(c => c.Id == claimId);
            if (claim != null)
            {
                claim.Status = newStatus;
                claim.LastUpdated = DateTime.Now;
                _ctx.SaveChanges();
            }
        }

        // FIX: Return user claims (list) instead of throwing
        public IEnumerable<Claim> GetUserClaims(string userId) =>
            _ctx.Claims
                 .Where(c => c.UserId == userId)
                 .Include(c => c.Documents)
                 .OrderByDescending(c => c.ClaimDate)
                 .ToList();

        // DTO for approved claims report
        public record ApprovedClaimRow(string ClaimId, string LecturerEmail, int Hours, decimal Rate, decimal Total, DateTime Date);

        //  Report method used by HR Approved page & ReportController
        public IEnumerable<ApprovedClaimRow> GetApprovedClaimsReport(DateTime? from, DateTime? to)
        {
            var query = _ctx.Claims.Where(c => c.Status == ClaimStatus.Approved);

            if (from.HasValue)
                query = query.Where(c => c.ClaimDate >= from.Value.Date);
            if (to.HasValue)
                query = query.Where(c => c.ClaimDate <= to.Value.Date);

            var result = from c in query
                         join u in _ctx.Users on c.LecturerId equals u.Id
                         select new ApprovedClaimRow(
                             c.ClaimId,
                             u.Email,
                             c.HoursWorked,
                             c.HourlyRate,
                             (c.TotalAmount > 0 ? c.TotalAmount : c.HoursWorked * c.HourlyRate),
                             c.ClaimDate
                         );

            return result
                .OrderBy(r => r.Date)
                .ThenBy(r => r.ClaimId)
                .ToList();
        }
    }
}