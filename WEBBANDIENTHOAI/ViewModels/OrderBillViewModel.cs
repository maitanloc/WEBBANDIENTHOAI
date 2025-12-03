// WEBBANDIENTHOAI/ViewModels/OrderBillViewModel.cs
namespace WEBBANDIENTHOAI.ViewModels
{
    public class OrderBillViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string ShippingAddress { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; }

        // ĐÃ SỬA: hiển thị tên thay vì số
        public string Status { get; set; }  // ví dụ: "Pending", "Delivered"...

        public List<OrderItemViewModel> OrderItems { get; set; }
    }

    public class OrderItemViewModel
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}