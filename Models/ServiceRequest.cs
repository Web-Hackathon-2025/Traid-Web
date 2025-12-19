using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Karigar.Models
{
    public enum RequestStatus
    {
        Requested = 1,
        Confirmed = 2,
        InProgress = 3,
        Completed = 4,
        Cancelled = 5,
        Rejected = 6
    }

    public class ServiceRequest
    {
        public int Id { get; set; }

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        [Required]
        public int ServiceId { get; set; }

        [ForeignKey("ServiceId")]
        public Service? Service { get; set; }

        [Required]
        public int ServiceProviderId { get; set; }

        [ForeignKey("ServiceProviderId")]
        public ServiceProvider? ServiceProvider { get; set; }

        [Required]
        public RequestStatus Status { get; set; } = RequestStatus.Requested;

        [Required]
        [StringLength(500)]
        public string ServiceAddress { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? SpecialInstructions { get; set; }

        [Required]
        public DateTime RequestedDate { get; set; }

        public DateTime? ConfirmedDate { get; set; }

        public DateTime? ScheduledDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public DateTime? CancelledDate { get; set; }

        [StringLength(500)]
        public string? CancellationReason { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? FinalPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public Review? Review { get; set; }
    }
}

