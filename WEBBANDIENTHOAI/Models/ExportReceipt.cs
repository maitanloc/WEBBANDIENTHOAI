using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Models
{
    // Entity chính - Map với bảng ExportReceipts
    public class ExportReceipt
    {
        [Key]
        public int ExportReceiptId { get; set; }

        [Required(ErrorMessage = "Số phiếu xuất là bắt buộc.")]
        [MaxLength(50, ErrorMessage = "Số phiếu xuất không được quá 50 ký tự.")]
        public string? ReceiptNumber { get; set; }

        public DateTime ExportDate { get; set; } = DateTime.UtcNow;

        public int? CustomerId { get; set; }

        public int? OrderId { get; set; }

        public int TotalQuantity { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalValue { get; set; } = 0m;

        public int CreatedByUserId { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        [ForeignKey("OrderId")]
        public Order? Order { get; set; }

        [ForeignKey("CreatedByUserId")]
        public User? CreatedByUser { get; set; }

        public ICollection<ExportReceiptDetail> ExportReceiptDetails { get; set; }
    }

    // DTO để gửi data vào stored procedure
    public class ExportReceiptCreateDto
    {
        public string? ReceiptNumber { get; set; }
        public int? CustomerId { get; set; }
        public int? OrderId { get; set; }
        public int CreatedByUserId { get; set; }
        public string? Notes { get; set; }
        public List<ExportItemDto> Items { get; set; } = new List<ExportItemDto>();
    }

    // DTO cho từng item xuất kho
    public class ExportItemDto
    {
        public int? ProductId { get; set; }
        public string? StockCode { get; set; }
        public string?   SKU { get; set; }
        public string? Name { get; set; }
        public string? Brand { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}