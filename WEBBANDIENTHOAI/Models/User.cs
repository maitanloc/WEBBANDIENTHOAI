using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBANDIENTHOAI.Models
{
    // Users (Admin / Staff)
    public class User
    {
        [Key]
        public int UserId { get; set; } // UserId: Mã user

        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc.")]
        [MaxLength(100, ErrorMessage = "Tên đăng nhập không được quá 100 ký tự.")]
        public string Username { get; set; } // Username: tên đăng nhập

        [Required]
        public byte[] PasswordHash { get; set; } // PasswordHash: SHA256 hash

        [MaxLength(150, ErrorMessage = "Họ tên không được quá 150 ký tự.")]
        public string FullName { get; set; } // FullName: tên đầy đủ

        [MaxLength(150, ErrorMessage = "Email không được quá 150 ký tự.")]
        [EmailAddress(ErrorMessage = "Định dạng email không hợp lệ.")]
        public string Email { get; set; } // Email

        public int RoleId { get; set; }
        public virtual Role Role { get; set; } // <-- navigation property

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
