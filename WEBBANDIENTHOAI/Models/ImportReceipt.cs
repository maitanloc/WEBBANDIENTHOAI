using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBANDIENTHOAI.Models
{
    public class ImportReceipt
    {
        [Key]
        public int ImportReceiptId { get; set; } // (ImportReceiptId)

        [Required(ErrorMessage = "Số phiếu nhập là bắt buộc.")]
        [MaxLength(50, ErrorMessage = "Số phiếu nhập không được quá 50 ký tự.")]
        public string ReceiptNumber { get; set; } // (ReceiptNumber)

        public DateTime ImportDate { get; set; } = DateTime.UtcNow; // (ImportDate)

        [MaxLength(200)]
        public string? SupplierName { get; set; } // (SupplierName)

        public int? TotalQuantity { get; set; } = 0; // (TotalQuantity)

        [Column(TypeName = "decimal(18,2)")]
        public decimal? TotalValue { get; set; } = 0m; // (TotalValue)

        public int? CreatedByUserId { get; set; } // (CreatedByUserId)

        [MaxLength(500)]
        public string? Notes { get; set; } // (Notes)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // (CreatedAt)
        public bool IsFinalized { get; set; } = false;
        public DateTime? LastUpdated { get; set; } 
        public ICollection<ImportReceiptDetail> Details { get; set; }
    }
}
