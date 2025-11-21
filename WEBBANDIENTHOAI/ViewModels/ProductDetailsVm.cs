using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.ViewModels
{
    public class ProductDetailsVm
    {
        public int ProductId { get; set; }
        public string SKU { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Brand { get; set; }
        public decimal Price { get; set; }
        public decimal? OldPrice { get; set; }
        public string StockCode { get; set; } = "";
        public string? Color { get; set; }
        public string? Size { get; set; }
        public string? ShortDescription { get; set; }
        public byte StatusId { get; set; }
        public string? StatusName { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? PrimaryImageId { get; set; }
        public List<ProductImageVm> Images { get; set; } = new();
        public LaptopConfigurationVm? LaptopConfiguration { get; set; }
        public PhoneConfigurationVm? PhoneConfiguration { get; set; }
    }
}
