using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanDienThoai.Models
{
    public class Inventory
    {
        [Key]
        public int InventoryId { get; set; } // (InventoryId)

        [Required, MaxLength(50)]
        public string StockCode { get; set; } // (StockCode: unique)

        public int? ProductId { get; set; } // (ProductId: có thể null nếu hàng chưa map)

        public int CurrentQuantity { get; set; } = 0; // (CurrentQuantity)

        public int MinimumQuantity { get; set; } = 0; // (MinimumQuantity)

        public int MaximumQuantity { get; set; } = 1000; // (MaximumQuantity)

        [MaxLength(100)]
        public string Location { get; set; } // (Location)

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow; // (LastUpdated)

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        public ICollection<InventoryHistory> History { get; set; }
    }
}
