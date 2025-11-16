using System.ComponentModel.DataAnnotations;

namespace WEBBANDIENTHOAI.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; } // (CategoryId)

        [Required, MaxLength(120)]
        public string CategoryName { get; set; } // (CategoryName)

        [MaxLength(500)]
        public string Description { get; set; } // (Description)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // (CreatedAt)

        public ICollection<Product> Products { get; set; }
    }
}
