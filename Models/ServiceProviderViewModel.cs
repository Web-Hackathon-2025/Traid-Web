using System.ComponentModel.DataAnnotations;

namespace Karigar.Models
{
    public class ServiceProviderViewModel
    {
        public int Id { get; set; }
        public string BusinessName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? ProfileImageUrl { get; set; }
        public double? AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<ServiceViewModel> Services { get; set; } = new();
        public double? Distance { get; set; }
    }

    public class ServiceViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? PriceUnit { get; set; }
        public bool IsAvailable { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}

