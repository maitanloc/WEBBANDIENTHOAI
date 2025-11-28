using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace WEBBANDIENTHOAI.Controllers
{
    public class WarrantyController : Controller
    {
        
        public IActionResult Index()
        {
            ViewData["Title"] = "Chính Sách Giao Hàng - FOXMOBILE";
            return View("~/Views/ChinhSach/ChinhSachBaoHanh.cshtml");
        }
    }
}