namespace WEBBANDIENTHOAI.ViewModels
{
    public class ProductListItemVm
    {
        public int ProductId { get; set; }
        public string SKU { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Brand { get; set; }
        public decimal Price { get; set; }
        public string StockCode { get; set; } = "";
        public byte StatusId { get; set; }
        public string? StatusName { get; set; }
        public int? PrimaryImageId { get; set; }
        public bool IsEditable { get; set; } = true;
    }
}
