using System.ComponentModel.DataAnnotations;

namespace WEBBANDIENTHOAI.ViewModels
{
    public class RoleViewModel
    {
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Tên vai trò là bắt buộc")]
        [StringLength(50, ErrorMessage = "Tên vai trò không được vượt quá 50 ký tự")]
        [Display(Name = "Tên vai trò")]
        public string RoleName { get; set; }

        [StringLength(250, ErrorMessage = "Mô tả không được vượt quá 250 ký tự")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }
    }
}