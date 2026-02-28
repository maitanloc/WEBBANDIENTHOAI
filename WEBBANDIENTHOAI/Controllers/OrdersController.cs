using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Repository.NguoiDung;
using WEBBANDIENTHOAI.Repository.TaiKhoan; // Add this using statement
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using WEBBANDIENTHOAI.Services;

namespace WEBBANDIENTHOAI.Controllers
{
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ICustomerRepository _customerRepository; 
        private readonly ICartRepository _cartRepository;
        private readonly IPromotionService _promotionService;

        public OrdersController(AppDbContext context, 
            ICustomerRepository customerRepository, 
            ICartRepository cartRepository,
            IPromotionService promotionService)
        {
            _context = context;
            _customerRepository = customerRepository;
            _cartRepository = cartRepository;
            _promotionService = promotionService;
        }

        private int? GetCurrentCustomerId()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("RoleName");

            if (!string.IsNullOrEmpty(userIdString) && role == "Customer")
            {
                if (int.TryParse(userIdString, out int customerId))
                {
                    return customerId;
                }
            }
            return null;
        }

        public async Task<IActionResult> HomeOrders(string searchQuery = null, string statusFilter = null)
        {
            var customerId = GetCurrentCustomerId();
            if (customerId == null)
            {
                return RedirectToAction("LoginRegister", "Account");
            }

            var customer = await _customerRepository.GetCustomerByIdAsync(customerId.Value);
            if (customer != null)
            {
                ViewBag.CustomerName = customer.FullName;
                ViewBag.CustomerEmail = customer.Email;
                ViewBag.CustomerPhone = customer.Phone;
                ViewBag.IsLoggedIn = true;
            }
            else
            {
                ViewBag.IsLoggedIn = false;
            }

            ViewBag.CurrentSearch = searchQuery;
            ViewBag.CurrentStatus = statusFilter;
            ViewBag.Statuses = await _context.OrderStatuses.ToListAsync();


            var ordersQuery = _context.Orders
                .Where(o => o.CustomerId == customerId.Value)
                .Include(o => o.OrderStatus)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                        .ThenInclude(p => p.PrimaryImage)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                var normalizedQuery = searchQuery.ToLower().Trim();
                ordersQuery = ordersQuery.Where(o =>
                    o.OrderId.ToString().Contains(normalizedQuery) ||
                    o.OrderDetails.Any(d => d.Product.Name.ToLower().Contains(normalizedQuery))
                );
            }

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "Tất cả")
            {
                ordersQuery = ordersQuery.Where(o => o.OrderStatus.StatusName == statusFilter);
            }

            var orders = await ordersQuery.OrderByDescending(o => o.OrderDate).ToListAsync();

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var customerId = GetCurrentCustomerId();
            if (customerId == null)
            {
                return Unauthorized(new { success = false, message = "Bạn cần đăng nhập để thực hiện việc này." });
            }

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == id && o.CustomerId == customerId.Value);

            if (order == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy đơn hàng." });
            }

            // Only allow cancellation if the status is Pending or Processing
            if (order.StatusId != 1 && order.StatusId != 2) // Assuming 1=Pending, 2=Processing
            {
                return BadRequest(new { success = false, message = "Không thể hủy đơn hàng ở trạng thái này." });
            }

            var cancelledStatus = await _context.OrderStatuses.FirstOrDefaultAsync(s => s.StatusName == "Cancelled");
            if (cancelledStatus == null)
            {
                // This would be an internal server error
                return StatusCode(500, new { success = false, message = "Trạng thái 'Cancelled' không được cấu hình trong hệ thống." });
            }

            order.StatusId = cancelledStatus.StatusId;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đơn hàng đã được hủy." });
        }
        [HttpPost]
        public async Task<IActionResult> ConfirmDelivery(int id)
        {
            var customerId = GetCurrentCustomerId();
            if (customerId == null)
            {
                return Unauthorized(new { success = false, message = "Bạn cần đăng nhập để thực hiện việc này." });
            }

            var order = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == id && o.CustomerId == customerId.Value);

            if (order == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy đơn hàng." });
            }

            // Assuming StatusId 3 is "Shipping"
            if (order.StatusId == 3 || order.StatusId == 2)
            {
                order.StatusId = 4; // Delivered
                await _context.SaveChangesAsync();

                // Cộng điểm F-Point
                int pointsEarned = await _promotionService.AwardPointsAsync(id);

                return Json(new { 
                    success = true, 
                    message = "Xác nhận đã nhận hàng thành công.",
                    pointsEarned = pointsEarned
                });
            }

            return BadRequest(new { success = false, message = "Không thể xác nhận đơn hàng ở trạng thái này." });
        }
        [HttpPost]
        public async Task<IActionResult> Repurchase(int id)
        {
            var customerId = GetCurrentCustomerId();
            if (customerId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để mua hàng!", requireLogin = true });
            }

            var order = await _context.Orders
                                      .Include(o => o.OrderDetails)
                                      .FirstOrDefaultAsync(o => o.OrderId == id && o.CustomerId == customerId.Value);

            if (order == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy đơn hàng." });
            }

            foreach (var item in order.OrderDetails)
            {
                await _cartRepository.AddToCartAsync(customerId.Value, item.ProductId, item.Quantity);
            }

            return Json(new { success = true, redirectToUrl = Url.Action("Index", "HomeCarts") });
        }
    }
}
