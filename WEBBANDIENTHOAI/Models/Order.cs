// WEBBANDIENTHOAI/Models/Order.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBANDIENTHOAI.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public int CustomerId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; } = 0m;

        // ĐÃ THAY ĐỔI: dùng StatusId thay vì Status string
        public int StatusId { get; set; } = 1; // 1 = Pending

        [MaxLength(300)]
        public string ShippingAddress { get; set; }

        public int? CreatedByUserId { get; set; }
        
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = "COD";

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [ForeignKey("StatusId")]
        public OrderStatus OrderStatus { get; set; }  // Mới thêm

        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
    // DTO cho thống kê đơn hàng
    public class OrderStatisticsDto
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
    }

    // DTO cho cập nhật trạng thái đơn hàng
    public class OrderStatusUpdateDto
    {
        [Required(ErrorMessage = "Vui lòng chọn trạng thái")]
        public int StatusId { get; set; }
    }
}