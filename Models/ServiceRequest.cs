using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Karigar.Models
{
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
        public ServiceProviderModel? ServiceProvider { get; set; }

        [Required]
        [StringLength(500)]
        public string ServiceAddress { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? SpecialInstructions { get; set; }

        [StringLength(500)]
        public string? CancellationReason { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Requested;

        public DateTime RequestedDate { get; set; } = DateTime.UtcNow;

        public DateTime? ScheduledDate { get; set; }

        public DateTime? ConfirmedDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public DateTime? CancelledDate { get; set; }

        public decimal? FinalPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Review? Review { get; set; }
    }

    public enum RequestStatus
    {
        Requested = 1,
        Confirmed = 2,
        InProgress = 3,
        Completed = 4,
        Cancelled = 5,
        Rejected = 6
    }
}
