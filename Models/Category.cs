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

        [StringLength(10)]
        public string? Icon { get; set; }
    }
}
