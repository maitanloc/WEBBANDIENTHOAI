using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBANDIENTHOAI.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required, StringLength(100)]
        public string Username { get; set; } = string.Empty;

        // store hashed password bytes
        [Required]
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

        [StringLength(150)]
        public string? FullName { get; set; }

        [StringLength(150)]
        public string? Email { get; set; }

        [ForeignKey(nameof(Role))]
        public int RoleId { get; set; }
        public virtual Role? Role { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
