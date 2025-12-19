using System.ComponentModel.DataAnnotations;

namespace Karigar.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Icon { get; set; }

        public ICollection<Service> Services { get; set; } = new List<Service>();
    }
}

