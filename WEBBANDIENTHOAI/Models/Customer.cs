using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBANDIENTHOAI.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Họ và tên là bắt buộc.")]
        [MaxLength(150, ErrorMessage = "Họ và tên không được vượt quá 150 ký tự.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
        [MaxLength(150)]
        public string Email { get; set; }

        [Required]
        public byte[] PasswordHash { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [MaxLength(30)]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Số CCCD/CMND là bắt buộc.")]
        [RegularExpression(@"^\d{9,12}$", ErrorMessage = "CCCD/CMND phải là 9 hoặc 12 chữ số.")]
        [MaxLength(12)]
        public string CitizenID { get; set; }

        [MaxLength(300, ErrorMessage = "Địa chỉ không được vượt quá 300 ký tự.")]
        public string Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // ===== LOYALTY =====
        /// <summary>Tổng điểm thưởng tích lũy</summary>
        public int LoyaltyPoints { get; set; } = 0;

        /// <summary>FK đến hạng thành viên hiện tại (1=Member, 2=Silver, 3=Gold, 4=Diamond)</summary>
        public int TierId { get; set; } = 1;

        /// <summary>Tổng chi tiêu tích lũy (dùng để tính hạng)</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalSpent { get; set; } = 0m;

        [ForeignKey("TierId")]
        public virtual CustomerTier? Tier { get; set; }

        public virtual ICollection<UserVoucher> UserVouchers { get; set; } = new List<UserVoucher>();
        public virtual ICollection<UserPointHistory> PointHistories { get; set; } = new List<UserPointHistory>();

        [Column(TypeName = "decimal(18,8)")]
        public decimal? Latitude { get; set; }

        [Column(TypeName = "decimal(18,8)")]
        public decimal? Longitude { get; set; }
    }
}