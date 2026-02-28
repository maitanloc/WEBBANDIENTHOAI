// WEBBANDIENTHOAI/Models/UserPointHistory.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBANDIENTHOAI.Models
{
    /// <summary>
    /// Lịch sử cộng / trừ điểm thưởng của khách hàng
    /// Points > 0 = cộng điểm, Points < 0 = trừ điểm
    /// </summary>
    public class UserPointHistory
    {
        [Key]
        public int HistoryId { get; set; }

        public int CustomerId { get; set; }

        /// <summary>Đơn hàng liên quan (không bắt buộc – có thể là đổi quà, thưởng sự kiện)</summary>
        public int? OrderId { get; set; }

        /// <summary>Điểm thay đổi: dương = cộng, âm = trừ</summary>
        public int Points { get; set; }

        [MaxLength(300)]
        public string Reason { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }
    }
}
