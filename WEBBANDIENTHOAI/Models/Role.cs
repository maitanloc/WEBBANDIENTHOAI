using System.ComponentModel.DataAnnotations;

namespace WEBBANDIENTHOAI.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; } // RoleId: Mã quyền

        [Required, MaxLength(50)]
        public string RoleName { get; set; } // RoleName: Admin/Staff/Customer

        [MaxLength(250)]
        public string Description { get; set; }
    }
}
