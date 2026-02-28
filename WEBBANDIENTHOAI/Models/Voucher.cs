// WEBBANDIENTHOAI/Models/Voucher.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBANDIENTHOAI.Models
{
    public enum DiscountType
    {
        Percent = 1,
        Fixed   = 2
    }

    /// <summary>
    /// Voucher / mã giảm giá thông minh
    /// </summary>
    public class Voucher
    {
        [Key]
        public int VoucherId { get; set; }

        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        /// <summary>Percent = giảm theo %, Fixed = giảm tiền cố định</summary>
        public DiscountType DiscountType { get; set; } = DiscountType.Fixed;

        /// <summary>Giá trị giảm: nếu Percent thì 10 = 10%, nếu Fixed thì 50000 = 50.000đ</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Value { get; set; }

        /// <summary>Giảm tối đa (VD: giảm 10% nhưng tối đa 100.000đ). Chỉ áp dụng với loại Percent</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? MaxDiscountAmount { get; set; }

        /// <summary>Giá trị đơn hàng tối thiểu để áp dụng</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal MinOrderValue { get; set; } = 0m;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        /// <summary>Tổng số lượt sử dụng tối đa (0 = không giới hạn)</summary>
        public int Quantity { get; set; } = 0;

        /// <summary>Số lượt đã sử dụng</summary>
        public int UsedCount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
