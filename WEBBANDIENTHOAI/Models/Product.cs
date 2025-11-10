using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;



namespace WEBBANDIENTHOAI.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [ForeignKey(nameof(Category))]
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        [Required, StringLength(60)]
        public string SKU { get; set; } = string.Empty;

        [Required, StringLength(250)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Brand { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? OldPrice { get; set; }

        public int Stock { get; set; }

        [StringLength(100)]
        public string? Color { get; set; }

        [StringLength(100)]
        public string? Size { get; set; }

        [StringLength(300)]
        public string? DefaultImage { get; set; }

        [StringLength(1000)]
        public string? ShortDescription { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<ProductImage>? ProductImages { get; set; }
        public virtual ICollection<CartDetail>? CartDetails { get; set; }
        public virtual ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}
