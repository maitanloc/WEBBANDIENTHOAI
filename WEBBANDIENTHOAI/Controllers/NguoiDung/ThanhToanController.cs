using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.ViewModels;
using System.Linq;
using System.Text.Json;

namespace WEBBANDIENTHOAI.Controllers.NguoiDung
{
    public class ThanhToanController : Controller
    {
        private readonly AppDbContext _context;
        private const string CheckoutSessionKey = "CheckoutData";

        public ThanhToanController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string selectedProductIds)
        {
            var customerId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(customerId) || role != "Customer")
            {
                return RedirectToAction("Login", "Account");
            }

            var productIds = selectedProductIds?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList() ?? new List<int>();

            if (!productIds.Any())
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một sản phẩm để thanh toán";
                return RedirectToAction("Index", "HomeCarts");
            }

            var customer = _context.Customers.Find(int.Parse(customerId));
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = _context.Carts
                .Include(c => c.Details)
                .ThenInclude(cd => cd.Product)
                .FirstOrDefault(c => c.CustomerId == int.Parse(customerId));

            if (cart == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy giỏ hàng";
                return RedirectToAction("Index", "HomeCarts");
            }

            var selectedItems = cart.Details
                .Where(cd => productIds.Contains(cd.ProductId))
                .Select(cd => new WEBBANDIENTHOAI.ViewModels.CartItemViewModel
                {
                    CartDetailId = cd.CartDetailId,
                    ProductId = cd.ProductId,
                    ProductName = cd.Product.Name ?? "Sản phẩm không tồn tại",
                    ProductImage = cd.Product.ImageId.HasValue ? $"/Image/ProductImage/{cd.Product.ImageId.Value}" : "/Images/default.jpg",
                    Price = cd.UnitPrice,
                    Quantity = cd.Quantity
                })
                .ToList();

            var viewModel = new CheckoutViewModel
            {
                Customer = customer,
                SelectedItems = selectedItems,
                TotalAmount = selectedItems.Sum(item => item.Price * item.Quantity),
                Phone = customer.Phone ?? "",
                FullName = customer.FullName,
                Address = customer.Address ?? "",
                Email = customer.Email
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Preview(CheckoutViewModel model)
        {
            var customerId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Validate required fields
            if (string.IsNullOrEmpty(model.FullName) || string.IsNullOrEmpty(model.Phone) ||
                string.IsNullOrEmpty(model.Address) || string.IsNullOrEmpty(model.Email))
            {
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin bắt buộc";
                return View("Index", model);
            }

            // Lấy lại thông tin sản phẩm từ database để đảm bảo tính nhất quán
            var productIds = model.SelectedItems.Select(item => item.ProductId).ToList();
            var products = _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ToDictionary(p => p.ProductId, p => new { p.Name, p.ImageId });

            // Cập nhật thông tin sản phẩm
            foreach (var item in model.SelectedItems)
            {
                if (products.ContainsKey(item.ProductId))
                {
                    var product = products[item.ProductId];
                    item.ProductName = product.Name;
                    item.ProductImage = product.ImageId.HasValue
                        ? $"/Image/ProductImage/{product.ImageId.Value}"
                        : "/Images/default.jpg";
                }
            }

            // Lưu tạm thông tin thanh toán vào session
            HttpContext.Session.SetString(CheckoutSessionKey, JsonSerializer.Serialize(model));

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProcessOrder()
        {
            var customerId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin thanh toán từ session
            var modelJson = HttpContext.Session.GetString(CheckoutSessionKey);
            if (string.IsNullOrEmpty(modelJson))
            {
                TempData["ErrorMessage"] = "Thông tin thanh toán không tồn tại hoặc đã hết hạn";
                return RedirectToAction("Index", "HomeCarts");
            }

            var model = JsonSerializer.Deserialize<CheckoutViewModel>(modelJson);

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // 1. Tạo đơn hàng mới
                var order = new Order
                {
                    CustomerId = int.Parse(customerId),
                    OrderDate = DateTime.UtcNow,
                    Total = model.TotalAmount,
                    Status = "Pending",
                    ShippingAddress = model.Address,
                    CreatedByUserId = null,
                    PaymentMethod = model.PaymentMethod,
                    Notes = model.Notes 
                };

                _context.Orders.Add(order);
                _context.SaveChanges();

                // 2. Tạo chi tiết đơn hàng
                foreach (var item in model.SelectedItems)
                {
                    var orderDetail = new OrderDetail
                    {
                        OrderId = order.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price
                    };
                    _context.OrderDetails.Add(orderDetail);
                }

                // 3. Xóa các sản phẩm đã thanh toán khỏi giỏ hàng
                var cart = _context.Carts
                    .Include(c => c.Details)
                    .FirstOrDefault(c => c.CustomerId == int.Parse(customerId));

                if (cart != null)
                {
                    var selectedCartDetails = cart.Details
                        .Where(cd => model.SelectedItems.Select(si => si.ProductId).Contains(cd.ProductId))
                        .ToList();

                    foreach (var cartDetail in selectedCartDetails)
                    {
                        _context.CartDetails.Remove(cartDetail);
                    }
                }

                _context.SaveChanges();
                transaction.Commit();

                // Xóa session sau khi xử lý thành công
                HttpContext.Session.Remove(CheckoutSessionKey);

                TempData["SuccessMessage"] = $"Đặt hàng thành công! Mã đơn hàng: #{order.OrderId}";
                return RedirectToAction("OrderSuccess", new { orderId = order.OrderId });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đặt hàng. Vui lòng thử lại.";
                return RedirectToAction("Index", new { selectedProductIds = string.Join(",", model.SelectedItems.Select(i => i.ProductId)) });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            // Xóa dữ liệu thanh toán khỏi session khi hủy
            HttpContext.Session.Remove(CheckoutSessionKey);

            TempData["InfoMessage"] = "Đã hủy đơn hàng. Bạn có thể chỉnh sửa thông tin và thử lại.";
            return RedirectToAction("Index", "HomeCarts");
        }

        public IActionResult OrderSuccess(int orderId)
        {
            // Lấy thông tin đơn hàng đầy đủ
            var order = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("Index", "HomeCarts");
            }

            // Tạo view model cho bill
            var billViewModel = new OrderBillViewModel
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                CustomerName = order.Customer.FullName,
                CustomerEmail = order.Customer.Email,
                CustomerPhone = order.Customer.Phone,
                ShippingAddress = order.ShippingAddress,
                TotalAmount = order.Total,
                PaymentMethod = order.PaymentMethod,
                Status = order.Status,
                OrderItems = order.OrderDetails.Select(od => new OrderItemViewModel
                {
                    ProductName = od.Product.Name,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,
                    TotalPrice = od.Quantity * od.UnitPrice
                }).ToList()
            };

            return View(billViewModel);
        }
    }
}