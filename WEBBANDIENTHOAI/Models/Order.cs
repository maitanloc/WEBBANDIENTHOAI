using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBANDIENTHOAI.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [ForeignKey(nameof(Customer))]
        public int CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending";

        [StringLength(300)]
        public string? ShippingAddress { get; set; }

        // optional: staff who created/processed order
        [ForeignKey(nameof(CreatedByUser))]
        public int? CreatedByUserId { get; set; }
        public virtual User? CreatedByUser { get; set; }

        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
