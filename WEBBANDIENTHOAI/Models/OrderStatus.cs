using System.ComponentModel.DataAnnotations;

namespace WEBBANDIENTHOAI.Models
{
    public class OrderStatus
    {
        [Key]
        public int StatusId { get; set; }

        [Required, StringLength(50)]
        public string StatusName { get; set; }

        [StringLength(200)]
        public string Description { get; set; }

        // Navigation
        public virtual ICollection<Order> Orders { get; set; }
    }
}