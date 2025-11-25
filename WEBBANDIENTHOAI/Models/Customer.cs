using System;
using System.ComponentModel.DataAnnotations;

namespace WEBBANDIENTHOAI.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required, MaxLength(150)]
        public string FullName { get; set; }

        [Required, MaxLength(150)]
        public string Email { get; set; }

        [Required]
        public byte[] PasswordHash { get; set; }

        [MaxLength(30)]
        public string Phone { get; set; }

        [Required, MaxLength(12)]
        public string CitizenID { get; set; }

        [MaxLength(300)]
        public string Address { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}