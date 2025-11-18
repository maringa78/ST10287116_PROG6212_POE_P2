using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ST10287116_PROG6212_POE_P2.Models
{
    public enum ClaimStatus { Pending, Verified, Approved, Rejected }  // Expanded for workflow (coordinator verifies, manager approves/rejects)

    public enum ClaimType { Travel, Equipment, Research, Conference, Development }

    public class Claim
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ClaimId { get; set; } = "CLM-" + DateTime.Now.ToString("yyyy-MM-dd");

        [Required]
        public DateTime ClaimDate { get; set; } = DateTime.Now;

        [Required]
        public ClaimType Type { get; set; }

        [Required, Range(0, double.MaxValue, ErrorMessage = "Amount must be positive")]
        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]  // Renders as "R 250.00" (fixes raw display)
        public decimal Amount { get; set; }  // Base amount

        [Required, StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public ClaimStatus Status { get; set; } = ClaimStatus.Pending;

        [Required, Range(0, int.MaxValue, ErrorMessage = "Hours must be non-negative")]
        public int HoursWorked { get; set; }  // NEW: Required per feedback

        [Required, Range(0, double.MaxValue, ErrorMessage = "Rate must be positive")]
        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public decimal HourlyRate { get; set; }  // NEW: Required per feedback

        [Column(TypeName = "decimal(18,2)")]
        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = true)]
        public decimal TotalAmount { get; set; }  // NEW: Calculated = Hours * Rate or Amount

        [NotMapped]  // Calculated, not stored
        public string ClaimMonth { get; set; } = DateTime.Now.ToString("MMMM yyyy");  // e.g., "November 2025"

        public string? UserId { get; set; }

        public List<Document> Documents { get; set; } = new();  // NEW: For uploads (viewable in admin)

        [Required]
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        [Required]
        public DateTime Created { get; set; } = DateTime.Now;

        [ForeignKey("Lecturer")]
        public int LecturerId { get; set; }  // Link to User (lecturer)
    }

    public class Document
    {
        public int Id { get; set; }

        [Required, StringLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required, StringLength(500)]
        public string FilePath { get; set; } = string.Empty;  // e.g., "/uploads/file.pdf" (viewable link)

        public int ClaimId { get; set; }

        public Claim Claim { get; set; } = null!;
    }
}