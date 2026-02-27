using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBANDIENTHOAI.Models
{
    public class CartDetail
    {
        [Key]
        public int CartDetailId { get; set; }

        public int CartId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        /// <summary>JSON lưu cấu hình đã chọn. Ví dụ: {"Màu sắc":"Titan Xanh","Bộ nhớ":"256GB"}</summary>
        [MaxLength(1000)]
        public string? SelectedOptions { get; set; }

        /// <summary>Tổng giá phụ thêm từ các tùy chọn cấu hình</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal OptionsPrice { get; set; } = 0m;

        [ForeignKey("CartId")]
        public virtual Cart Cart { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}
