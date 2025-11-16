using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBANDIENTHOAI.Models
{
    public class CartDetail
    {
        [Key]
        public int CartDetailId { get; set; } // (CartDetailId)

        public int CartId { get; set; } // (CartId)

        public int ProductId { get; set; } // (ProductId)

        public int Quantity { get; set; } = 1; // (Quantity)

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; } // (UnitPrice)

        [ForeignKey("CartId")]
        public Cart Cart { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
