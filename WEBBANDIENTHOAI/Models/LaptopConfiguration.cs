using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBANDIENTHOAI.Models
{
    public class LaptopConfiguration
    {
        [Key]
        public int ConfigurationId { get; set; } // (ConfigurationId)

        public int ProductId { get; set; } // (ProductId, unique)

        [MaxLength(200)]
        public string CPU { get; set; }

        [MaxLength(100)]
        public string RAM { get; set; }

        [MaxLength(200)]
        public string Storage { get; set; }

        [MaxLength(200)]
        public string GraphicsCard { get; set; }

        [MaxLength(100)]
        public string Battery { get; set; }

        [MaxLength(100)]
        public string OperatingSystem { get; set; }

        [MaxLength(50)]
        public string ScreenSize { get; set; }

        [MaxLength(100)]
        public string ScreenTechnology { get; set; }

        [MaxLength(100)]
        public string Resolution { get; set; }

        [MaxLength(500)]
        public string Ports { get; set; }

        [MaxLength(50)]
        public string Color { get; set; }

        [MaxLength(50)]
        public string Weight { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }
}
