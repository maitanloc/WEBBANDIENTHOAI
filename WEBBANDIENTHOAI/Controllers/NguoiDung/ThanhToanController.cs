using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.Json;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.ViewModels;

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
                return Json(new { success = false, message = "Vui lòng đăng nhập lại." });
            }

            // Lấy thông tin thanh toán từ session
            var modelJson = HttpContext.Session.GetString(CheckoutSessionKey);
            if (string.IsNullOrEmpty(modelJson))
            {
                return Json(new { success = false, message = "Thông tin thanh toán không tồn tại hoặc đã hết hạn." });
            }

            var model = JsonSerializer.Deserialize<CheckoutViewModel>(modelJson);

            try
            {
                int orderId = 0; // Khai báo orderId ngoài để sử dụng sau

                var strategy = _context.Database.CreateExecutionStrategy(); // Tạo execution strategy

                strategy.Execute(() => // Bọc toàn bộ transaction vào strategy để hỗ trợ retry
                {
                    using var transaction = _context.Database.BeginTransaction(); // Bắt đầu transaction

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
                        Notes = model.Notes ?? string.Empty // Set to empty string if null to avoid null issues
                    };

                    _context.Orders.Add(order);
                    _context.SaveChanges();

                    orderId = order.OrderId; // Gán orderId sau khi SaveChanges()

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
                    transaction.Commit(); // Commit transaction
                });

                // Xóa session sau khi xử lý thành công
                HttpContext.Session.Remove(CheckoutSessionKey);

                return Json(new { success = true, message = "Đặt hàng thành công!", redirectUrl = Url.Action("OrderSuccess", new { orderId }) });
            }
            catch (Exception ex)
            {
                // Không cần rollback thủ công vì strategy sẽ xử lý
                Console.WriteLine($"Error in ProcessOrder: {ex.Message} - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Có lỗi xảy ra khi đặt hàng: {ex.Message}. Vui lòng thử lại." });
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
                CustomerName = order.Customer?.FullName ?? string.Empty,
                CustomerEmail = order.Customer?.Email ?? string.Empty,
                CustomerPhone = order.Customer?.Phone ?? string.Empty,
                ShippingAddress = order.ShippingAddress ?? string.Empty,
                TotalAmount = order.Total,
                PaymentMethod = order.PaymentMethod ?? string.Empty,
                Status = order.Status ?? string.Empty,
                OrderItems = order.OrderDetails.Select(od => new OrderItemViewModel
                {
                    ProductName = od.Product?.Name ?? "Unknown",
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,
                    TotalPrice = od.Quantity * od.UnitPrice
                }).ToList()
            };

            return View(billViewModel);
        }
    }
}