using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Helpers;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.ViewModels; // THÊM USING NÀY

namespace WEBBANDIENTHOAI.Controllers
{
    public class HomeUserController : Controller
    {
        private readonly AppDbContext _context;

        public HomeUserController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Lấy sản phẩm bao gồm cả hình ảnh chính
            var featuredProducts = _context.Products
                .Include(p => p.PrimaryImage) // QUAN TRỌNG: Include hình ảnh
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .ToList();

            // Kiểm tra role
            var role = HttpContext.Session.GetString("RoleName");
            if (role != "Customer")
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy toàn bộ file banner trong folder wwwroot/images/banners
            var bannerFiles = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/banners"))
                                       .Select(Path.GetFileName)
                                       .ToList();

            ViewBag.Banners = bannerFiles;

            var allProducts = _context.Products.Take(20).ToList();

            return View(featuredProducts);
        }

        // ACTION CHI TIẾT SẢN PHẨM - THÊM VÀO ĐÂY
        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductStatus)
                .Include(p => p.PrimaryImage) // Eager load primary image
                .Include(p => p.Images) // Eager load image gallery
                .Include(p => p.PhoneConfiguration)
                .Include(p => p.LaptopConfiguration)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            // Tạo một danh sách các ảnh, bắt đầu với ảnh chính
            var orderedImages = new List<ProductImage>();
            if (product.PrimaryImage != null)
            {
                orderedImages.Add(product.PrimaryImage);
            }
            // Thêm các ảnh còn lại trong gallery (tránh trùng lặp)
            if (product.Images != null)
            {
                orderedImages.AddRange(product.Images.Where(img => img.ImageId != product.ImageId));
            }


            var viewModel = new ProductDetailsVm
            {
                ProductId = product.ProductId,
                SKU = product.SKU,
                Name = product.Name,
                Brand = product.Brand,
                Price = product.Price,
                OldPrice = product.OldPrice,
                StockCode = product.StockCode,
                Color = product.Color,
                Size = product.Size,
                ShortDescription = product.ShortDescription,
                StatusId = product.StatusId,
                StatusName = product.ProductStatus?.StatusName,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.CategoryName,
                CreatedAt = product.CreatedAt,
                PrimaryImageId = product.ImageId,
                Images = orderedImages.Select(img => new ProductImageVm { ImageId = img.ImageId }).ToList(),

                PhoneConfiguration = product.PhoneConfiguration != null ? new PhoneConfigurationVm
                {
                    CPU = product.PhoneConfiguration.CPU,
                    Cores = product.PhoneConfiguration.Cores,
                    RAM = product.PhoneConfiguration.RAM,
                    InternalStorage = product.PhoneConfiguration.InternalStorage,
                    Battery = product.PhoneConfiguration.Battery,
                    Camera = product.PhoneConfiguration.Camera
                } : null,

                LaptopConfiguration = product.LaptopConfiguration != null ? new LaptopConfigurationVm
                {
                    CPU = product.LaptopConfiguration.CPU,
                    RAM = product.LaptopConfiguration.RAM,
                    Storage = product.LaptopConfiguration.Storage,
                    Battery = product.LaptopConfiguration.Battery,
                    OperatingSystem = product.LaptopConfiguration.OperatingSystem,
                    ScreenSize = product.LaptopConfiguration.ScreenSize
                } : null,
            };

            return View(viewModel);
        }

        // Action để lấy sản phẩm liên quan (cho AJAX)
        public async Task<JsonResult> GetRelatedProducts(int categoryId, int excludeProductId, int count = 4)
        {
            var relatedProducts = await _context.Products
                .Include(p => p.PrimaryImage)
                .Where(p => p.CategoryId == categoryId && p.ProductId != excludeProductId)
                .OrderBy(p => Guid.NewGuid()) // Random order
                .Take(count)
                .Select(p => new
                {
                    productId = p.ProductId,
                    name = p.Name,
                    price = p.Price,
                    primaryImageId = p.ImageId
                })
                .ToListAsync();

            return Json(relatedProducts);
        }

        // Action để hiển thị hình ảnh từ database
        public IActionResult GetProductImage(int imageId)
        {
            var image = _context.ProductImages.FirstOrDefault(pi => pi.ImageId == imageId);

            if (image?.ImagePath != null && image.ImagePath.Length > 0)
            {
                // Sử dụng ImageHelper để xác định content type
                string contentType = ImageHelper.GetContentType(image.ImagePath);
                return File(image.ImagePath, contentType);
            }

            // Trả về hình ảnh mặc định nếu không tìm thấy
            return File("~/images/default-product.png", "image/png");
        }
    }
}