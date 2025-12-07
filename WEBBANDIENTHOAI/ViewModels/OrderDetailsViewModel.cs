using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.ViewModels
{
    public class OrderDetailsViewModel
    {
        // Thông tin đơn hàng
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }
        public int StatusId { get; set; }  // ← THÊM DÒNG NÀY
        public string StatusName { get; set; }
        public string Notes { get; set; }
        public decimal Total { get; set; }

        // Chi tiết sản phẩm
        public List<OrderDetailItem> OrderDetails { get; set; } = new List<OrderDetailItem>();
    }

    public class OrderDetailItem
    {
        public int OrderDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductImage { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => Quantity * UnitPrice;
    }
}