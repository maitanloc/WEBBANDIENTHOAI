using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.ViewModels;
using System.Linq;

namespace WEBBANDIENTHOAI.Controllers.NguoiDung
{
    public class OrderHistoryController : Controller
    {
        private readonly AppDbContext _context;

        public OrderHistoryController(AppDbContext context)
        {
            _context = context;
        }

        // TẤT CẢ ĐƠN HÀNG
        public async Task<IActionResult> Index()
        {
            return await LoadOrdersByStatus(null, "Tất Cả Đơn Hàng", "Xem toàn bộ lịch sử mua hàng của bạn");
        }

        // ĐƠN HÀNG ĐANG XỬ LÝ (Pending + Processing)
        public async Task<IActionResult> ProcessingOrders()
        {
            return await LoadOrdersByStatus(new[] { "Pending", "Processing" }, "Đơn Hàng Đang Xử Lý", "Theo dõi các đơn hàng đang được xử lý");
        }

        // ĐƠN HÀNG ĐANG GIAO (Shipped)
        public async Task<IActionResult> ShippingOrders()
        {
            return await LoadOrdersByStatus(new[] { "Shipped" }, "Đơn Hàng Đang Giao", "Theo dõi các đơn hàng đang trên đường vận chuyển");
        }

        // ĐƠN HÀNG ĐÃ HOÀN THÀNH (Completed)
        public async Task<IActionResult> CompletedOrders()
        {
            return await LoadOrdersByStatus(new[] { "Completed" }, "Đơn Hàng Đã Hoàn Thành", "Các đơn hàng đã được giao thành công");
        }

        // ĐƠN HÀNG ĐÃ HỦY (Cancelled)
        public async Task<IActionResult> CancelledOrders()
        {
            return await LoadOrdersByStatus(new[] { "Cancelled" }, "Đơn Hàng Đã Hủy", "Các đơn hàng đã bị hủy");
        }

        // PHƯƠNG THỨC TRUNG GIAN ĐỂ LOAD ĐƠN HÀNG THEO STATUS
        private async Task<IActionResult> LoadOrdersByStatus(string[] statuses, string pageTitle, string pageDescription)
        {
            try
            {
                // DEBUG: In ra session
                Console.WriteLine($"=== {pageTitle.ToUpper()} - SESSION DEBUG ===");
                foreach (var key in HttpContext.Session.Keys)
                {
                    Console.WriteLine($"{key}: {HttpContext.Session.GetString(key)}");
                }

                var userIdStr = HttpContext.Session.GetString("UserId");
                var customerIdStr = HttpContext.Session.GetString("CustomerId");
                var role = HttpContext.Session.GetString("RoleName");

                Console.WriteLine($"UserId: {userIdStr}, CustomerId: {customerIdStr}, Role: {role}");

                // CHỈ cho phép Customer truy cập
                if (string.IsNullOrEmpty(userIdStr) || role != "Customer")
                {
                    Console.WriteLine("=== REDIRECT TO LOGIN - NOT CUSTOMER ===");
                    TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem đơn hàng";
                    return RedirectToAction("Login", "Account");
                }

                // Sử dụng CustomerId nếu có, nếu không thì dùng UserId
                if (!int.TryParse(customerIdStr ?? userIdStr, out int customerId))
                {
                    Console.WriteLine("=== INVALID CUSTOMER ID ===");
                    TempData["ErrorMessage"] = "Thông tin khách hàng không hợp lệ";
                    return RedirectToAction("Login", "Account");
                }

                Console.WriteLine($"=== LOADING ORDERS FOR CUSTOMER: {customerId} ===");
                Console.WriteLine($"Status filter: {(statuses != null ? string.Join(", ", statuses) : "ALL")}");

                // Kiểm tra customer có tồn tại không
                var customerExists = await _context.Customers.AnyAsync(c => c.CustomerId == customerId);
                if (!customerExists)
                {
                    Console.WriteLine("=== CUSTOMER NOT FOUND IN DATABASE ===");
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng";
                    return RedirectToAction("Login", "Account");
                }

                // Lấy danh sách đơn hàng với filter theo status
                var query = _context.Orders
                    .Where(o => o.CustomerId == customerId);

                if (statuses != null && statuses.Length > 0)
                {
                    query = query.Where(o => statuses.Contains(o.Status));
                }

                var orders = await query
                    .Include(o => o.Customer)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                Console.WriteLine($"=== FOUND {orders.Count} ORDERS ===");

                // Tạo ViewModel
                var orderViewModels = orders.Select(o => new OrderHistoryViewModel
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.Total,
                    Status = o.Status ?? "Chưa xác định",
                    PaymentMethod = o.PaymentMethod ?? "COD",
                    ShippingAddress = o.ShippingAddress ?? "Chưa có địa chỉ",
                    Notes = o.Notes ?? "",
                    CustomerName = o.Customer?.FullName ?? "Khách hàng",
                    CustomerEmail = o.Customer?.Email ?? "",
                    CustomerPhone = o.Customer?.Phone ?? "",
                    Items = o.OrderDetails?.Select(od => new OrderHistoryItemViewModel
                    {
                        ProductName = od.Product?.Name ?? "Sản phẩm không tồn tại",
                        ProductImage = od.Product?.ImageId.HasValue == true
                            ? $"/Image/ProductImage/{od.Product.ImageId.Value}"
                            : "/Images/default.jpg",
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice
                    }).ToList() ?? new List<OrderHistoryItemViewModel>()
                }).ToList();

                var viewModel = new OrderHistoryIndexViewModel
                {
                    Orders = orderViewModels
                };

                // Truyền thông tin trang qua ViewBag
                ViewBag.PageTitle = pageTitle;
                ViewBag.PageDescription = pageDescription;
                ViewBag.UserName = HttpContext.Session.GetString("Username") ?? "Khách hàng";

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== ERROR IN {pageTitle.ToUpper()}: {ex.Message} ===");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                TempData["ErrorMessage"] = $"Có lỗi xảy ra khi tải {pageTitle.ToLower()}";
                return View("Index", new OrderHistoryIndexViewModel { Orders = new List<OrderHistoryViewModel>() });
            }
        }

        // CHI TIẾT ĐƠN HÀNG
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                var customerIdStr = HttpContext.Session.GetString("CustomerId");
                var role = HttpContext.Session.GetString("RoleName");

                if (string.IsNullOrEmpty(userIdStr) || role != "Customer")
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!int.TryParse(customerIdStr ?? userIdStr, out int customerId))
                {
                    TempData["ErrorMessage"] = "Thông tin khách hàng không hợp lệ";
                    return RedirectToAction("Index");
                }

                var order = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .Where(o => o.OrderId == id && o.CustomerId == customerId)
                    .FirstOrDefaultAsync();

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền truy cập";
                    return RedirectToAction("Index");
                }

                var orderViewModel = new OrderHistoryViewModel
                {
                    OrderId = order.OrderId,
                    OrderDate = order.OrderDate,
                    TotalAmount = order.Total,
                    Status = order.Status ?? "Chưa xác định",
                    PaymentMethod = order.PaymentMethod ?? "COD",
                    ShippingAddress = order.ShippingAddress ?? "Chưa có địa chỉ",
                    Notes = order.Notes ?? "",
                    CustomerName = order.Customer?.FullName ?? "Khách hàng",
                    CustomerEmail = order.Customer?.Email ?? "",
                    CustomerPhone = order.Customer?.Phone ?? "",
                    Items = order.OrderDetails?.Select(od => new OrderHistoryItemViewModel
                    {
                        ProductName = od.Product?.Name ?? "Sản phẩm không tồn tại",
                        ProductImage = od.Product?.ImageId.HasValue == true
                            ? $"/Image/ProductImage/{od.Product.ImageId.Value}"
                            : "/Images/default.jpg",
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice
                    }).ToList() ?? new List<OrderHistoryItemViewModel>()
                };

                ViewBag.UserName = HttpContext.Session.GetString("Username") ?? "Khách hàng";
                return View(orderViewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi tải chi tiết đơn hàng: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải chi tiết đơn hàng";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            try
            {
                var userIdStr = HttpContext.Session.GetString("UserId");
                var customerIdStr = HttpContext.Session.GetString("CustomerId");
                var role = HttpContext.Session.GetString("RoleName");

                if (string.IsNullOrEmpty(userIdStr) || role != "Customer")
                {
                    return RedirectToAction("Login", "Account");
                }

                if (!int.TryParse(customerIdStr ?? userIdStr, out int customerId))
                {
                    TempData["ErrorMessage"] = "Thông tin khách hàng không hợp lệ";
                    return RedirectToAction("Index");
                }

                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.OrderId == orderId && o.CustomerId == customerId);

                if (order == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                    return RedirectToAction("Index");
                }

                if (order.Status != "Pending")
                {
                    TempData["ErrorMessage"] = "Chỉ có thể hủy đơn hàng đang chờ xác nhận";
                    return RedirectToAction("Details", new { id = orderId });
                }

                order.Status = "Cancelled";
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã hủy đơn hàng thành công";
                return RedirectToAction("Details", new { id = orderId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi hủy đơn hàng: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi hủy đơn hàng";
                return RedirectToAction("Details", new { id = orderId });
            }
        }

        // TEST SESSION
        [HttpGet]
        public IActionResult Test()
        {
            var sessionInfo = new
            {
                UserId = HttpContext.Session.GetString("UserId"),
                CustomerId = HttpContext.Session.GetString("CustomerId"),
                RoleName = HttpContext.Session.GetString("RoleName"),
                Username = HttpContext.Session.GetString("Username"),
                AllKeys = HttpContext.Session.Keys.ToList()
            };

            Console.WriteLine("=== TEST SESSION ===");
            Console.WriteLine($"UserId: {sessionInfo.UserId}");
            Console.WriteLine($"CustomerId: {sessionInfo.CustomerId}");
            Console.WriteLine($"RoleName: {sessionInfo.RoleName}");
            Console.WriteLine($"All Keys: {string.Join(", ", sessionInfo.AllKeys)}");

            return Content($"Test - CustomerId: {sessionInfo.CustomerId}, Role: {sessionInfo.RoleName}");
        }
    }
}