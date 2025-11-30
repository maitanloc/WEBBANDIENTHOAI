using Microsoft.AspNetCore.Mvc;

namespace WEBBANDIENTHOAI.Controllers.ChinhSach
{
    public class ShippingController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Chính Sách Giao Hàng - FOXMOBILE";
            return View("~/Views/ChinhSach/ChinhSachGiaoHang.cshtml");
        }
    }
}
