using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanDienThoai.Models
{
    public class InventoryHistory
    {
        [Key]
        public int HistoryId { get; set; } // (HistoryId)

        public int InventoryId { get; set; } // (InventoryId)

        public int ChangeQuantity { get; set; } // (ChangeQuantity: +/-)

        [MaxLength(50)]
        public string Operation { get; set; } // (Operation: IMPORT/EXPORT/ADJUST/TRANSFER)

        public int? ReferenceId { get; set; } // (ReferenceId: id liên quan)

        [MaxLength(500)]
        public string Note { get; set; } // (Note)

        public int? CreatedByUserId { get; set; } // (CreatedByUserId)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // (CreatedAt)

        [ForeignKey("InventoryId")]
        public Inventory Inventory { get; set; }
    }
}
