using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanDienThoai.Models
{
    public class ExportReceipt
    {
        [Key]
        public int ExportReceiptId { get; set; } // (ExportReceiptId)

        [Required, MaxLength(50)]
        public string ReceiptNumber { get; set; } // (ReceiptNumber)

        public DateTime ExportDate { get; set; } = DateTime.UtcNow; // (ExportDate)

        public int? CustomerId { get; set; } // (CustomerId: nếu bán)

        public int? OrderId { get; set; } // (OrderId: liên kết đơn hàng)

        public int TotalQuantity { get; set; } = 0; // (TotalQuantity)

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalValue { get; set; } = 0m; // (TotalValue)

        public int CreatedByUserId { get; set; } // (CreatedByUserId)

        [MaxLength(500)]
        public string Notes { get; set; } // (Notes)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // (CreatedAt)

        public ICollection<ExportReceiptDetail> Details { get; set; }
    }
}
