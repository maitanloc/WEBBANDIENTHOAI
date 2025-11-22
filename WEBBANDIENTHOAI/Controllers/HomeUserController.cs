using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Helpers;
using WEBBANDIENTHOAI.Models;

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