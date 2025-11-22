using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.ViewModels
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        // Tính tổng tiền của item này
        public decimal TotalPrice => Price * Quantity;
    }

    public class CartIndexViewModel
    {
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();

        // Tổng tiền toàn bộ giỏ hàng
        public decimal GrandTotal => CartItems.Sum(x => x.TotalPrice);
    }
}
