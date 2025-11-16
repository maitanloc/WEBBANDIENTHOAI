using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBANDIENTHOAI.Models
{
    public class ProductImage
    {
        [Key]
        public int ImageId { get; set; } // (ImageId)

        public int ProductId { get; set; } // (ProductId)

        [Required, MaxLength(300)]
        public string ImagePath { get; set; } // (ImagePath)

        public bool IsPrimary { get; set; } = false; // (IsPrimary)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // (CreatedAt)

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
