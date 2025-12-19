using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Karigar.Models
{
    public class Dispute
    {
        public int Id { get; set; }

        [Required]
        public int ServiceRequestId { get; set; }

        [ForeignKey("ServiceRequestId")]
        public ServiceRequest? ServiceRequest { get; set; }

        [Required]
        public string ReportedByUserId { get; set; } = string.Empty;

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public DisputeStatus Status { get; set; } = DisputeStatus.Pending;

        [StringLength(1000)]
        public string? AdminNotes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }
    }

    public enum DisputeStatus
    {
        Pending = 1,
        UnderReview = 2,
        Resolved = 3,
        Dismissed = 4
    }
}

