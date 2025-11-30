using Microsoft.AspNetCore.Mvc;

namespace WEBBANDIENTHOAI.Controllers.ChinhSach
{
    public class PaymentController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Chính Sách Thanh Toán - FOXMOBILE";
            return View("~/Views/ChinhSach/ChinhSachThanhToan.cshtml");
        }
    }
}
