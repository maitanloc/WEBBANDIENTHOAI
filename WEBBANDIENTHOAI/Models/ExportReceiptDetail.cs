using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WEBBANDIENTHOAI.Models;

namespace WebBanDienThoai.Models
{
    public class ExportReceiptDetail
    {
        [Key]
        public int ExportDetailId { get; set; } // (ExportDetailId)

        public int ExportReceiptId { get; set; } // (ExportReceiptId)

        public int? ProductId { get; set; } // (ProductId)

        [Required, MaxLength(60)]
        public string SnapshotSKU { get; set; } // (SnapshotSKU)

        [MaxLength(250)]
        public string SnapshotName { get; set; } // (SnapshotName)

        [MaxLength(100)]
        public string SnapshotBrand { get; set; } // (SnapshotBrand)

        [Required, MaxLength(50)]
        public string StockCode { get; set; } // (StockCode)

        public int Quantity { get; set; } = 0; // (Quantity)

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } = 0m; // (UnitPrice)

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; } = 0m; // (TotalPrice)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // (CreatedAt)

        [ForeignKey("ExportReceiptId")]
        public ExportReceipt ExportReceipt { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
