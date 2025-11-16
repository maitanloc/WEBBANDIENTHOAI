using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebBanDienThoai.Models;
using WEBBANDIENTHOAI.Models;

public class Product
{
    [Key]
    public int ProductId { get; set; } // (ProductId)

    public int CategoryId { get; set; } // (CategoryId)

    [Required, MaxLength(60)]
    public string SKU { get; set; } // (SKU: mã SKU, KHÔNG NÊN THAY)

    [Required, MaxLength(250)]
    public string Name { get; set; } // (Name)

    [MaxLength(100)]
    public string Brand { get; set; } // (Brand)

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; } = 0m; // (Price)

    [Column(TypeName = "decimal(18,2)")]
    public decimal? OldPrice { get; set; } // (OldPrice)

    [Required, MaxLength(50)]
    public string StockCode { get; set; } // (StockCode: mã tồn kho, KHÔNG NÊN THAY nếu có tồn)

    [MaxLength(100)]
    public string Color { get; set; } // (Color)

    [MaxLength(100)]
    public string Size { get; set; } // (Size)

    [MaxLength(300)]
    public string DefaultImage { get; set; } // (DefaultImage: đường dẫn ảnh chính)

    [MaxLength(1000)]
    public string ShortDescription { get; set; } // (ShortDescription)

    public byte StatusId { get; set; } = 1; // (StatusId: FK trạng thái)

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // (CreatedAt)

    // Navigation
    [ForeignKey("CategoryId")]
    public Category Category { get; set; }

    [ForeignKey("StatusId")]
    public ProductStatus ProductStatus { get; set; }

    public ICollection<ProductImage> Images { get; set; }
    public LaptopConfiguration LaptopConfiguration { get; set; }
    public PhoneConfiguration PhoneConfiguration { get; set; }
    public ICollection<ImportReceiptDetail> ImportDetails { get; set; }
    public ICollection<ExportReceiptDetail> ExportDetails { get; set; }
    public ICollection<OrderDetail> OrderDetails { get; set; }
    public ICollection<CartDetail> CartDetails { get; set; }
}
