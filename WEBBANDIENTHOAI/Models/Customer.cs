using System;
using System.ComponentModel.DataAnnotations;

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
    }
}