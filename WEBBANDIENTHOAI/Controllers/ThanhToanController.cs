using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.ViewModels;
using System.Linq;

namespace WEBBANDIENTHOAI.Controllers
{
    public class ThanhToanController : Controller
    {
        private readonly AppDbContext _context;

        public ThanhToanController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Index(string selectedProductIds)
        {
            var customerId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(customerId) || role != "Customer")
            {
                return RedirectToAction("Login", "Account");
            }

            // Chuyển đổi chuỗi selectedProductIds thành List<int>
            var productIds = selectedProductIds?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList() ?? new List<int>();

            // Kiểm tra nếu không có sản phẩm nào được chọn
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

            // Lấy giỏ hàng và các sản phẩm được chọn
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
                .Select(cd => new WEBBANDIENTHOAI.Models.CartItemViewModel
                {
                    CartDetailId = cd.CartDetailId,
                    ProductId = cd.ProductId,
                    ProductName = cd.Product.Name ?? "Sản phẩm không tồn tại",
                    ImageUrl = cd.Product.ImageId.HasValue ? $"/Image/ProductImage/{cd.Product.ImageId.Value}" : "/Images/default.jpg",
                    Price = cd.UnitPrice,
                    Quantity = cd.Quantity
                })
                .ToList();

            var viewModel = new CheckoutViewModel
            {
                Customer = customer,
                SelectedItems = selectedItems,
                TotalAmount = selectedItems.Sum(item => item.Price * item.Quantity),
                Phone = customer.Phone,
                FullName = customer.FullName,
                Address = customer.Address,
                Email = customer.Email
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult ProcessOrder(CheckoutViewModel model)
        {
            var customerId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Login", "Account");
            }

            using var transaction = _context.Database.BeginTransaction();

            try
            {
                // 1. Tạo đơn hàng mới
                var order = new Order
                {
                    CustomerId = int.Parse(customerId),
                    OrderDate = DateTime.UtcNow,
                    Total = model.TotalAmount,
                    Status = "Pending", // Chờ xác nhận
                    ShippingAddress = model.Address,
                    CreatedByUserId = null // Có thể set nếu có user hệ thống
                };

                _context.Orders.Add(order);
                _context.SaveChanges(); // Lưu để lấy OrderId

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

                TempData["SuccessMessage"] = $"Đặt hàng thành công! Mã đơn hàng: #{order.OrderId}";
                return RedirectToAction("OrderSuccess", new { orderId = order.OrderId });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                // Log lỗi ex ở đây
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi đặt hàng. Vui lòng thử lại.";
                return View("Index", model);
            }
        }

        public IActionResult OrderSuccess(int orderId)
        {
            ViewBag.OrderId = orderId;
            return View();
        }
    }
}