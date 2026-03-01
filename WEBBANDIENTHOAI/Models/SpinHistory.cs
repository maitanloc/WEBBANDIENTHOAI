// WEBBANDIENTHOAI/Models/SpinHistory.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBANDIENTHOAI.Models
{
    /// <summary>
    /// Lịch sử quay vòng quay may mắn của khách hàng
    /// </summary>
    public class SpinHistory
    {
        [Key]
        public int SpinId { get; set; }

        public int CustomerId { get; set; }

        /// <summary>Voucher trúng (null nếu không trúng)</summary>
        public int? VoucherId { get; set; }

        public DateTime SpinAt { get; set; } = DateTime.UtcNow;

        /// <summary>Tên hiển thị của ô trúng (VD: "Giảm 10%", "Chúc may mắn lần sau")</summary>
        [MaxLength(200)]
        public string SpinResult { get; set; } = string.Empty;

        // Navigation
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        [ForeignKey("VoucherId")]
        public virtual Voucher? Voucher { get; set; }
    }
}
