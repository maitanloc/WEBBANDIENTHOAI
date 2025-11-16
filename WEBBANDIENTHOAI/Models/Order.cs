using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WEBBANDIENTHOAI.Models;

public class Order
{
    [Key]
    public int OrderId { get; set; } // (OrderId)

    public int CustomerId { get; set; } // (CustomerId)

    public DateTime OrderDate { get; set; } = DateTime.UtcNow; // (OrderDate)

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; } = 0m; // (Total)

    [MaxLength(50)]
    public string Status { get; set; } = "Pending"; // (Status)

    [MaxLength(300)]
    public string ShippingAddress { get; set; } // (ShippingAddress)

    public int? CreatedByUserId { get; set; } // (CreatedByUserId)

    [ForeignKey("CustomerId")]
    public Customer Customer { get; set; }

    public ICollection<OrderDetail> Details { get; set; }
}
