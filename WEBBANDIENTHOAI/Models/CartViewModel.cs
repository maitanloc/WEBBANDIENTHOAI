namespace WEBBANDIENTHOAI.Models
{
    public class CartViewModel
    {
        public int CartId { get; set; }
        public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
        public decimal SubTotal => Items.Sum(i => i.Total);
        public decimal Discount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal Total => SubTotal - Discount + ShippingFee;
        public int TotalItems => Items.Sum(i => i.Quantity);
    }

    public class CartItemViewModel
    {
        public int CartDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total => UnitPrice * Quantity;
        public bool IsSelected { get; set; } = true;
    }
}