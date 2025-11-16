using System.ComponentModel.DataAnnotations;

namespace WEBBANDIENTHOAI.Models
{
    public class ProductStatus
    {
        [Key]
        public byte StatusId { get; set; } // (StatusId: TINYINT)

        [Required, MaxLength(30)]
        public string StatusName { get; set; } // (StatusName)

        [MaxLength(150)]
        public string Description { get; set; } // (Description)

        public ICollection<Product> Products { get; set; }
    }
}
