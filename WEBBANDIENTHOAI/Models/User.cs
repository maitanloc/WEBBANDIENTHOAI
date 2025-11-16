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

        [Required, MaxLength(100)]
        public string Username { get; set; } // Username: tên đăng nhập

        [Required]
        public byte[] PasswordHash { get; set; } // PasswordHash: SHA256 hash

        [MaxLength(150)]
        public string FullName { get; set; } // FullName: tên đầy đủ

        [MaxLength(150)]
        public string Email { get; set; } // Email

        public int RoleId { get; set; }
        public Role Role { get; set; } // <-- navigation property

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
