using System.ComponentModel.DataAnnotations;
namespace WEBBANDIENTHOAI.Dtos
{
    public class ProductEditDto
    {
        public int ProductId { get; set; }
        [Required, MaxLength(60)]
        public string SKU { get; set; } = null!;
        [Required, MaxLength(250)]
        public string Name { get; set; } = null!;
        [MaxLength(100)]
        public string? Brand { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required, MaxLength(50)]
        public string StockCode { get; set; } = null!;
        [MaxLength(100)]
        public string? Color { get; set; }
        [MaxLength(100)]
        public string? Size { get; set; }
        [MaxLength(1000)]
        public string? ShortDescription { get; set; }
        public byte StatusId { get; set; }
    }
}