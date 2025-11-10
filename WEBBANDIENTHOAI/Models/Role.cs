
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace WEBBANDIENTHOAI.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required, StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        // Navigation
        public virtual ICollection<User>? Users { get; set; }
    }
}
