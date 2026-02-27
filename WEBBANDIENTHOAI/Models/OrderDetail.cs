using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBANDIENTHOAI.Models
{
    public class OrderDetail
    {
        [Key]
        public int OrderDetailId { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [StringLength(255)]
        public string ProductName { get; set; } = string.Empty;

        /// <summary>Snapshot cấu hình đã chọn khi đặt hàng. Ví dụ: {"Màu sắc":"Titan Xanh","Bộ nhớ":"256GB"}</summary>
        [MaxLength(1000)]
        public string? SelectedOptions { get; set; }

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}