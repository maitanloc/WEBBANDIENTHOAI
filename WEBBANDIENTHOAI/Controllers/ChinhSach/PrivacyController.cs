using Microsoft.AspNetCore.Mvc;

namespace WEBBANDIENTHOAI.Controllers.ChinhSach
{
    public class PrivacyController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Chính Sách Bảo Mật - FOXMOBILE";
            return View("~/Views/ChinhSach/ChinhSachBaoMat.cshtml");
        }
    }
}