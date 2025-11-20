using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBBANDIENTHOAI.Models
{
    public class ProductStatus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public byte StatusId { get; set; }

        [Required, MaxLength(30)]
        public string StatusName { get; set; }

        [MaxLength(150)]
        public string Description { get; set; }

        public ICollection<Product> Products { get; set; }
    }

    // ==================== DTOs ====================

    public class ProductStatusDto
    {
        public byte StatusId { get; set; }

        [Required(ErrorMessage = "Tên trạng thái là bắt buộc")]
        [StringLength(30, ErrorMessage = "Tên trạng thái không quá 30 ký tự")]
        public string StatusName { get; set; } = null!;

        [StringLength(150, ErrorMessage = "Mô tả không quá 150 ký tự")]
        public string? Description { get; set; }

        public int ProductCount { get; set; }
    }

    public class CreateProductStatusDto
    {
        [Required(ErrorMessage = "Tên trạng thái là bắt buộc")]
        [StringLength(30, ErrorMessage = "Tên trạng thái không quá 30 ký tự")]
        public string StatusName { get; set; } = null!;

        [StringLength(150, ErrorMessage = "Mô tả không quá 150 ký tự")]
        public string? Description { get; set; }
    }

    public class UpdateProductStatusDto
    {
        public byte StatusId { get; set; }

        [Required(ErrorMessage = "Tên trạng thái là bắt buộc")]
        [StringLength(30, ErrorMessage = "Tên trạng thái không quá 30 ký tự")]
        public string StatusName { get; set; } = null!;

        [StringLength(150, ErrorMessage = "Mô tả không quá 150 ký tự")]
        public string? Description { get; set; }
    }
}