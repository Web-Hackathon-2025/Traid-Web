using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Karigar.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public int ServiceRequestId { get; set; }

        [ForeignKey("ServiceRequestId")]
        public ServiceRequest? ServiceRequest { get; set; }

        [Required]
        public string CustomerId { get; set; } = string.Empty;

        [Required]
        public int ServiceProviderId { get; set; }

        [ForeignKey("ServiceProviderId")]
        public ServiceProvider? ServiceProvider { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsVisible { get; set; } = true;
    }
}

