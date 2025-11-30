using Microsoft.AspNetCore.Mvc;
using WEBBANDIENTHOAI.Data;

namespace WEBBANDIENTHOAI.Controllers.TaiKhoan
{
    public class CustomerController : Controller
    {
         private readonly AppDbContext _context;
             
    public CustomerController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Customer
    public IActionResult Index()
    {
        var list = _context.Customers.ToList();

           
            return View(list);
    }
    }
}
