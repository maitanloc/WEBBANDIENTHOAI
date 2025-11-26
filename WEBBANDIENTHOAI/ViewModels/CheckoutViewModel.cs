using WEBBANDIENTHOAI.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WEBBANDIENTHOAI.ViewModels
{
    public class CheckoutViewModel
    {
        public Customer Customer { get; set; }
        public List<WEBBANDIENTHOAI.Models.CartItemViewModel> SelectedItems { get; set; } // Chỉ rõ namespace
        public decimal TotalAmount { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        public string Notes { get; set; }
        public string PaymentMethod { get; set; }
    }
}