using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBANDIENTHOAI.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; } // (CartId)

        public int CustomerId { get; set; } // (CustomerId)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // (CreatedAt)

        public DateTime? UpdatedAt { get; set; } // (UpdatedAt)

        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; }

        public virtual ICollection<CartDetail> Details { get; set; }
    }

}
