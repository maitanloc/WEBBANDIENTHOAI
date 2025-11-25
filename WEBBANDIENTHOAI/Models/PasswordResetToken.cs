using System;
using System.ComponentModel.DataAnnotations;

namespace WEBBANDIENTHOAI.Models
{
    public class PasswordResetToken
    {
        [Key]
        public int TokenId { get; set; }

        [Required, MaxLength(100)]
        public string Token { get; set; }

        [Required, MaxLength(150)]
        public string Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;
    }
}