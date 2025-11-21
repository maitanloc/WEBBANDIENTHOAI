using Microsoft.AspNetCore.Mvc;

namespace WEBBANDIENTHOAI.Controllers
{
    public class InventoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
