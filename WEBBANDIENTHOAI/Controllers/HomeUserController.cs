using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;

namespace WEBBANDIENTHOAI.Controllers
{
    public class HomeUserController : Controller
    {
        private readonly AppDbContext _context;

        // ⭐ BẮT BUỘC PHẢI CÓ CONSTRUCTOR NÀY
        public HomeUserController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Lấy 8 sản phẩm nổi bật – hoặc tất cả
            var featuredProducts = _context.Products
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .ToList();

            // Lấy role từ session
            var role = HttpContext.Session.GetString("RoleName");

            // Nếu không phải Customer -> quay về Login
            if (role != "Customer")
            {
                return RedirectToAction("Login", "Account");
            }

            return View(featuredProducts); // nhớ trả data ra View
        }
    }
}
