using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBANDIENTHOAI.Models
{
    /// <summary>
    /// Lưu các tùy chọn cấu hình của sản phẩm điện thoại/laptop.
    /// Ví dụ: Nhóm "Màu sắc" -> ["Titan Tự Nhiên", "Titan Xanh", "Titan Đen", "Titan Trắng"]
    ///         Nhóm "RAM"      -> ["8GB (+0đ)", "16GB (+2,000,000đ)"]
    ///         Nhóm "Bộ nhớ"  -> ["128GB", "256GB (+2,000,000đ)", "512GB (+5,000,000đ)"]
    /// </summary>
    public class ProductOption
    {
        [Key]
        public int ProductOptionId { get; set; }

        /// <summary>FK tới sản phẩm</summary>
        public int ProductId { get; set; }

        /// <summary>Tên nhóm tùy chọn. Ví dụ: "Màu sắc", "RAM", "Bộ nhớ trong"</summary>
        [Required]
        [MaxLength(100)]
        public string GroupName { get; set; } = null!;

        /// <summary>Tên lựa chọn cụ thể. Ví dụ: "Titan Xanh", "8GB", "256GB"</summary>
        [Required]
        [MaxLength(150)]
        public string OptionName { get; set; } = null!;

        /// <summary>Giá phụ thêm so với giá gốc sản phẩm (có thể = 0)</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal AdditionalPrice { get; set; } = 0m;

        /// <summary>Thứ tự hiển thị trong nhóm</summary>
        public int DisplayOrder { get; set; } = 0;

        /// <summary>Còn hiệu lực hay không</summary>
        public bool IsActive { get; set; } = true;

        // Navigation property
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }
    }
}
