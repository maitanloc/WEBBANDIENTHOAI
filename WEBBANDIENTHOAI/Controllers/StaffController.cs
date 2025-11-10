using Microsoft.AspNetCore.Mvc;
using WEBBANDIENTHOAI.Filters;

namespace WEBBANDIENTHOAI.Controllers
{
    [RequireRole("Staff,Admin")] // cho phép Staff và Admin
    public class StaffController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // Thêm action quản lý sản phẩm, duyệt đơn... (mẫu)
        public IActionResult Products()
        {
            // gọi DB lấy list products...
            return View();
        }
    }
}
