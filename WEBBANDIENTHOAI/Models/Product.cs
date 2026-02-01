using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required, MaxLength(60)]
        public string SKU { get; set; } = null!;

        [Required, MaxLength(250)]
        public string Name { get; set; } = null!;

        [MaxLength(100)]
        public string? Brand { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OldPrice { get; set; }

        [Required, MaxLength(50)]
        public string StockCode { get; set; } = null!;

        [MaxLength(100)]
        public string? Color { get; set; }

        [MaxLength(100)]
        public string? Size { get; set; }

        public int? ImageId { get; set; }

        [MaxLength(1000)]
        public string? ShortDescription { get; set; }

        public byte StatusId { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("CategoryId")]
        public virtual Category? Category { get; set; }

        [ForeignKey("StatusId")]
        public virtual ProductStatus? ProductStatus { get; set; }

        [ForeignKey("ImageId")]
        public virtual ProductImage? PrimaryImage { get; set; }

        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public virtual LaptopConfiguration? LaptopConfiguration { get; set; }
        public virtual PhoneConfiguration? PhoneConfiguration { get; set; }
        public virtual ICollection<ImportReceiptDetail> ImportDetails { get; set; } = new List<ImportReceiptDetail>();
        public virtual ICollection<ExportReceiptDetail> ExportDetails { get; set; } = new List<ExportReceiptDetail>();
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public virtual ICollection<CartDetail> CartDetails { get; set; } = new List<CartDetail>();
        public virtual ICollection<Inventory> Inventory { get; set; } = new List<Inventory>();
    }
}