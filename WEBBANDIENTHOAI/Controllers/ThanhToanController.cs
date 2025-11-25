using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Controllers
{
    public class ThanhToanController : Controller
    {
        private readonly AppDbContext _context;

        public ThanhToanController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var customerId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("RoleName");

            // Kiểm tra nếu chưa đăng nhập hoặc không phải customer thì chuyển hướng
            if (string.IsNullOrEmpty(customerId) || role != "Customer")
            {
                return RedirectToAction("Login", "Account");
            }

            // Tìm thông tin khách hàng
            var customer = _context.Customers.Find(int.Parse(customerId));
            if (customer == null)
            {
                // Nếu không tìm thấy, có thể xử lý bằng cách tạo mới hoặc báo lỗi, ở đây chúng ta chuyển hướng về login
                return RedirectToAction("Login", "Account");
            }

            return View(customer);
        }
    }
}