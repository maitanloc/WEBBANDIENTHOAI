using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBANDIENTHOAI.Models
{
    public class PhoneConfiguration
    {
        [Key]
        public int ConfigurationId { get; set; } // (ConfigurationId)

        public int ProductId { get; set; } // (ProductId)

        [MaxLength(200)]
        public string CPU { get; set; }

        [MaxLength(50)]
        public string Cores { get; set; }

        [MaxLength(50)]
        public string Threads { get; set; }

        [MaxLength(100)]
        public string RAM { get; set; }

        [MaxLength(100)]
        public string InternalStorage { get; set; }

        [MaxLength(100)]
        public string Battery { get; set; }

        [MaxLength(100)]
        public string OperatingSystem { get; set; }

        [MaxLength(200)]
        public string Screen { get; set; }

        [MaxLength(100)]
        public string ScreenTechnology { get; set; }

        [MaxLength(100)]
        public string Resolution { get; set; }

        [MaxLength(500)]
        public string Camera { get; set; }

        [MaxLength(500)]
        public string Ports { get; set; }

        [MaxLength(50)]
        public string Color { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("ProductId")]
        public Product Product { get; set; }
    }

}
