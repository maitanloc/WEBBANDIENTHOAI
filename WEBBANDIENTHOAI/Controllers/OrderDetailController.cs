using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Controllers
{
    public class OrderDetailController : Controller
    {
        private readonly AppDbContext _context;

        public OrderDetailController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> HomeOrderDetail(int id)
        {
            var order = await _context.Orders
                                    .Include(o => o.OrderDetails)
                                        .ThenInclude(od => od.Product)
                                            .ThenInclude(p => p.PrimaryImage)
                                    .Include(o => o.OrderStatus)
                                    .Include(o => o.Customer)
                                    .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
