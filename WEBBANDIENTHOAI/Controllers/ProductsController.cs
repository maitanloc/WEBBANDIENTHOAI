using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using WEBBANDIENTHOAI.Data; // namespace của AppDbContext
using WEBBANDIENTHOAI.Models;
using System.Linq;

namespace WEBBANDIENTHOAI.Controllers
{
    public class ProductsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(AppDbContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET /Products
        public async Task<IActionResult> Index()
        {
            try
            {
                // lấy danh sách (giới hạn 100 để an toàn)
                var products = await _context.Products
                    .Include(p => p.PrimaryImage)
                    .Include(p => p.ProductStatus)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(100)
                    .ToListAsync();

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    Response.Headers["X-Page-Title"] = "Danh sách sản phẩm";
                    return PartialView("_ProductsIndexPartial", products);
                }

                ViewData["Title"] = "Danh sách sản phẩm";
                return View("Index", products); // full page fallback if needed
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in Products.Index");
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Content("<div class='p-6 text-red-600'>Lỗi khi tải sản phẩm. Vui lòng kiểm tra log server.</div>", "text/html");
                return StatusCode(500);
            }
        }

        // GET /Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Images)
                    .Include(p => p.PrimaryImage)
                    .Include(p => p.PhoneConfiguration)
                    .Include(p => p.LaptopConfiguration)
                    .Include(p => p.ProductStatus)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null)
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                        return Content("<div class='p-6 text-gray-600'>Không tìm thấy sản phẩm.</div>", "text/html");
                    return NotFound();
                }

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    Response.Headers["X-Page-Title"] = product.Name;
                    return PartialView("_ProductDetailsPartial", product);
                }

                ViewData["Title"] = product.Name;
                return View("Details", product);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in Products.Details({Id})", id);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Content("<div class='p-6 text-red-600'>Lỗi khi tải chi tiết sản phẩm.</div>", "text/html");
                return StatusCode(500);
            }
        }

        // Serve image bytes from ProductImages table
        public async Task<IActionResult> Image(int id)
        {
            try
            {
                var image = await _context.ProductImages.FirstOrDefaultAsync(pi => pi.ImageId == id);
                if (image == null || image.ImagePath == null) return NotFound();

                // image.ImagePath is VARBINARY -> byte[]
                var bytes = image.ImagePath;
                string contentType = "image/jpeg";
                if (bytes.Length >= 8 && bytes[0] == 0x89 && bytes[1] == 0x50) contentType = "image/png";
                return File(bytes, contentType);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error in Products.Image({Id})", id);
                return NotFound();
            }
        }
    }
}
