using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBANDIENTHOAI.Models
{
    /// <summary>
    /// Quy tắc bán chéo sản phẩm (Cross-selling).
    /// Khi khách mua TriggerProduct → gợi ý thêm SuggestedProduct với giá ưu đãi.
    /// Ví dụ: Mua iPhone 15 Pro → Gợi ý Ốp lưng chỉ 99.000đ thay vì 199.000đ
    /// </summary>
    public class CrossSellRule
    {
        [Key]
        public int CrossSellRuleId { get; set; }

        /// <summary>Sản phẩm kích hoạt quy tắc (VD: iPhone 15 Pro)</summary>
        public int TriggerProductId { get; set; }

        /// <summary>Sản phẩm được gợi ý (VD: Ốp lưng Silicon)</summary>
        public int SuggestedProductId { get; set; }

        /// <summary>Giá ưu đãi khi mua kèm (hiển thị là giá ưu đãi)</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountedPrice { get; set; }

        /// <summary>
        /// Thông điệp gợi ý hiển thị cho khách.
        /// Ví dụ: "Hoàn thiện trải nghiệm iPhone của bạn!"
        /// </summary>
        [MaxLength(500)]
        public string? DisplayMessage { get; set; }

        /// <summary>Quy tắc đang hiệu lực hay không</summary>
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [ForeignKey("TriggerProductId")]
        public virtual Product? TriggerProduct { get; set; }

        [ForeignKey("SuggestedProductId")]
        public virtual Product? SuggestedProduct { get; set; }
    }
}
