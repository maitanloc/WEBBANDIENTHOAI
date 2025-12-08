using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBANDIENTHOAI.Models
{
    // Entity - Map với bảng ExportReceiptDetails
    public class ExportReceiptDetail
    {
        [Key]
        public int ExportDetailId { get; set; }

        public int ExportReceiptId { get; set; }

        public int? ProductId { get; set; }

        [Required]
        [MaxLength(50)]
        public string? StockCode { get; set; }

        [Required]
        [MaxLength(60)]
        public string? SnapshotSKU { get; set; }

        [MaxLength(250)]
        public string? SnapshotName { get; set; }

        [MaxLength(100)]
        public string? SnapshotBrand { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("ExportReceiptId")]
        public ExportReceipt ExportReceipt { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}