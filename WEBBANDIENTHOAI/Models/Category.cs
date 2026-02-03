using System.ComponentModel.DataAnnotations;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc.")]
        [MaxLength(120, ErrorMessage = "Tên danh mục không được quá 120 ký tự.")]
        public string CategoryName { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Product> Products { get; set; }
    }

    // ==================== DTOs ====================

    public class CategoryDto
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(120, ErrorMessage = "Tên danh mục không quá 120 ký tự")]
        public string CategoryName { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Mô tả không quá 500 ký tự")]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public int ProductCount { get; set; }
    }

    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(120, ErrorMessage = "Tên danh mục không quá 120 ký tự")]
        public string CategoryName { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Mô tả không quá 500 ký tự")]
        public string? Description { get; set; }
    }

    public class UpdateCategoryDto
    {
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
        [StringLength(120, ErrorMessage = "Tên danh mục không quá 120 ký tự")]
        public string CategoryName { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Mô tả không quá 500 ký tự")]
        public string? Description { get; set; }
    }
}