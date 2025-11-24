using System.Collections.Generic;
using System.Linq;

namespace WEBBANDIENTHOAI.ViewModels
{
    public class CartItemViewModel
    {
        public int CartDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => Price * Quantity;
    }

    public class CartIndexViewModel
    {
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
        public decimal GrandTotal => CartItems.Sum(x => x.TotalPrice);
        public int TotalItems => CartItems.Sum(x => x.Quantity);
    }
}