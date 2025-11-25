using System;
using System.ComponentModel.DataAnnotations;

namespace WEBBANDIENTHOAI.Models
{
    public class OTPCode
    {
        [Key]
        public int OTPId { get; set; }

        [Required, MaxLength(6)]
        public string Code { get; set; }

        [Required, MaxLength(150)]
        public string Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public int Attempts { get; set; } = 0;
    }
}