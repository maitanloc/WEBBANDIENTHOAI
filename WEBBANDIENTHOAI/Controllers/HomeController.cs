using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("UserId") == null)
                return RedirectToAction("Login", "Account");

            var role = HttpContext.Session.GetString("RoleName");
            if (role == "Admin") return RedirectToAction("Index", "Admin", new { area = "Admin" });
            if (role == "Staff") return RedirectToAction("Index", "Staff", new { area = "Admin" });
            
            return RedirectToAction("Index", "HomeUser");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
