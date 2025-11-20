using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WEBBANDIENTHOAI.Models;

public class OrderDetail
{
    [Key]
    public int OrderDetailId { get; set; } // (OrderDetailId)

    public int OrderId { get; set; } // (OrderId)

    public int ProductId { get; set; } // (ProductId)

    public int Quantity { get; set; } // (Quantity)

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; } // (UnitPrice)

    [ForeignKey("OrderId")]
    public Order Order { get; set; }

    [ForeignKey("ProductId")]
    public Product Product { get; set; }
}
