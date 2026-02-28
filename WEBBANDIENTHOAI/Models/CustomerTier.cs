// WEBBANDIENTHOAI/Models/CustomerTier.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBANDIENTHOAI.Models
{
    /// <summary>
    /// Bảng hạng thành viên: Member, Silver, Gold, Diamond
    /// </summary>
    public class CustomerTier
    {
        [Key]
        public int TierId { get; set; }

        [Required, MaxLength(50)]
        public string TierName { get; set; } = string.Empty;

        /// <summary>Tổng chi tiêu tối thiểu để đạt hạng này</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal MinSpending { get; set; } = 0m;

        /// <summary>Hệ số nhân điểm thưởng (VD: 1.0 = bình thường, 1.5 = nhân 1.5 lần)</summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal BonusMultiplier { get; set; } = 1.0m;

        [MaxLength(200)]
        public string? Description { get; set; }

        // Navigation
        public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }
}
