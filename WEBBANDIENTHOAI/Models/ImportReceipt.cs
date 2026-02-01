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

        [Required, MaxLength(50)]
        public string ReceiptNumber { get; set; } // (ReceiptNumber)

        public DateTime ImportDate { get; set; } = DateTime.UtcNow; // (ImportDate)

        [MaxLength(200)]
        public string SupplierName { get; set; } // (SupplierName)

        public int TotalQuantity { get; set; } = 0; // (TotalQuantity)

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalValue { get; set; } = 0m; // (TotalValue)

        public int CreatedByUserId { get; set; } // (CreatedByUserId)

        [MaxLength(500)]
        public string Notes { get; set; } // (Notes)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // (CreatedAt)
        public DateTime? LastUpdated { get; set; } // (LastUpdated)
        public bool IsFinalized { get; set; } = false; // (IsFinalized)

        // Navigation
        public ICollection<ImportReceiptDetail> Details { get; set; }
    }
}
