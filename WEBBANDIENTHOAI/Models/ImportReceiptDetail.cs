using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Models
{
    public class ImportReceiptDetail
    {
        [Key]
        public int ImportDetailId { get; set; } // (ImportDetailId)

        public int ImportReceiptId { get; set; } // (ImportReceiptId)

        public int? ProductId { get; set; } // (ProductId: mapping nếu có)

        [Required, MaxLength(60)]
        public string? SnapshotSKU { get; set; } // (SnapshotSKU)

        [MaxLength(250)]
        public string? SnapshotName { get; set; } // (SnapshotName)

        [MaxLength(100)]
        public string? SnapshotBrand { get; set; } // (SnapshotBrand)

        [Required, MaxLength(50)]
        public string? StockCode { get; set; } // (StockCode)

        public int Quantity { get; set; } = 0; // (Quantity)

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; } = 0m; // (UnitCost)

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; } = 0m; // (TotalCost)

        [MaxLength(100)]
        public string? BatchNumber { get; set; } // (BatchNumber)

        public DateTime? ExpiryDate { get; set; } // (ExpiryDate)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // (CreatedAt)

        [ForeignKey("ImportReceiptId")]
        public ImportReceipt ImportReceipt { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
