using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBANDIENTHOAI.Models
{
    public class ProductImage
    {
        [Key]
        public int ImageId { get; set; } // (ImageId)

        public int ProductId { get; set; } // (ProductId)

        // Đúng với cột VARBINARY(MAX) trong DB
        public byte[]? ImagePath { get; set; }

        public bool IsPrimary { get; set; } = false; // (IsPrimary)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // (CreatedAt)

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
