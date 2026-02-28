// WEBBANDIENTHOAI/Models/UserVoucher.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBANDIENTHOAI.Models
{
    /// <summary>
    /// Kho voucher của từng khách hàng (1 customer – N voucher – M-N)
    /// </summary>
    public class UserVoucher
    {
        [Key]
        public int UserVoucherId { get; set; }

        public int CustomerId { get; set; }
        public int VoucherId { get; set; }

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public bool IsUsed { get; set; } = false;
        public DateTime? UsedAt { get; set; }

        /// <summary>Đơn hàng đã áp dụng voucher này (null nếu chưa dùng)</summary>
        public int? OrderId { get; set; }

        // Navigation
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        [ForeignKey("VoucherId")]
        public virtual Voucher Voucher { get; set; } = null!;

        [ForeignKey("OrderId")]
        public virtual Order? Order { get; set; }
    }
}
