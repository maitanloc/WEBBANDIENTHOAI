using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.ViewModels
{
    public class CartItemViewModel
    {
        public int CartDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; } // Đảm bảo dùng ProductImage thay vì ImageUrl
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int InventoryQuantity { get; set; }
        public decimal TotalPrice => Price * Quantity;
    }

    public class CartIndexViewModel
    {
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
        public decimal GrandTotal => CartItems.Sum(x => x.TotalPrice);
        public int TotalItems => CartItems.Sum(x => x.Quantity);
    }

    // Thêm CheckoutViewModel vào cùng file để thống nhất
    public class CheckoutViewModel
    {
        public Customer Customer { get; set; }
        public List<CartItemViewModel> SelectedItems { get; set; }
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        public string Address { get; set; }

        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public string Notes { get; set; }
        public string PaymentMethod { get; set; } = "COD";

    }
}