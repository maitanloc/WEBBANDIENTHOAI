using Microsoft.AspNetCore.Mvc;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Repository.Admin;
using WEBBANDIENTHOAI.Repository.TaiKhoan;
using WEBBANDIENTHOAI.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace WEBBANDIENTHOAI.Controllers.Admin
{
    public class OrderController : Controller
    {
        private readonly IOrderRepository        _orderRepository;
        private readonly IOrderStatusRepository   _orderStatusRepository;
        private readonly ICustomerRepository      _customerRepository;
        private readonly IOrderDetailsRepository  _orderDetailsRepository;
        private readonly IPromotionService        _promotionSvc;

        public OrderController(
            IOrderRepository orderRepository,
            IOrderStatusRepository orderStatusRepository,
            ICustomerRepository customerRepository,
            IOrderDetailsRepository orderDetailsRepository,
            IPromotionService promotionSvc)
        {
            _orderRepository        = orderRepository;
            _orderStatusRepository  = orderStatusRepository;
            _customerRepository     = customerRepository;
            _orderDetailsRepository = orderDetailsRepository;
            _promotionSvc           = promotionSvc;
        }

        // GET: Admin/Order
        [Route("Admin/Order")]
        [Route("Admin/Order/Index")]
        public async Task<IActionResult> Index(
            string search = "",
            int? customerId = null,
            string paymentMethod = "",
            int? statusId = null,
            string startDate = "",
            string endDate = "",
            string sortBy = "newest",
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                ViewBag.Search = search;
                ViewBag.CustomerId = customerId;
                ViewBag.PaymentMethod = paymentMethod;
                ViewBag.StatusId = statusId;
                ViewBag.StartDate = !string.IsNullOrEmpty(startDate) ? DateTime.Parse(startDate) : (DateTime?)null;
                ViewBag.EndDate = !string.IsNullOrEmpty(endDate) ? DateTime.Parse(endDate) : (DateTime?)null;
                ViewBag.SortBy = sortBy;
                ViewBag.CurrentPage = page;

                var statuses = await _orderStatusRepository.GetAllAsync();
                ViewBag.Statuses = statuses;

                ViewBag.Customers = new List<Customer>();

                var orders = await _orderRepository.GetOrdersAsync(
                    search, paymentMethod, statusId, startDate, endDate, sortBy, page, pageSize);

                ViewBag.TotalRevenue = orders.Items.Sum(o => o.Total);

                var statistics = await _orderRepository.GetStatisticsAsync();
                ViewBag.Statistics = statistics;

                return View(orders);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return View(new WEBBANDIENTHOAI.Repository.Admin.PagedResult<Order>());
            }
        }

        // POST: Admin/Order/UpdateStatus
        [HttpPost]
        [Route("Admin/Order/UpdateStatus")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int OrderId, int StatusId)
        {
            try
            {
                if (OrderId <= 0 || StatusId <= 0)
                {
                    TempData["Error"] = "Thông tin không hợp lệ";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _orderRepository.UpdateStatusAsync(OrderId, StatusId);

                if (result)
                {
                    TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công!";

                    // ===== LOYALTY HOOK: cộng / trừ điểm theo trạng thái =====
                    try
                    {
                        if (StatusId == 4) // Delivered
                        {
                            await _promotionSvc.AwardPointsAsync(OrderId);
                        }
                        else if (StatusId == 5 || StatusId == 6) // Cancelled or Returned
                        {
                            await _promotionSvc.RevokePointsAsync(OrderId);
                        }
                    }
                    catch (Exception loyaltyEx)
                    {
                        // Lỗi loyalty không nên đổ ngã toàn bộ request
                        Console.WriteLine($"[LoyaltyHook] Error: {loyaltyEx.Message}");
                    }
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng để cập nhật";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Order/GetOrderDetails - AJAX endpoint
        [HttpGet]
        [Route("Admin/Order/GetOrderDetails")]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            try
            {
                // Log để debug
                System.Diagnostics.Debug.WriteLine($"=== GetOrderDetails called with orderId={orderId} ===");

                if (orderId <= 0)
                {
                    return Json(new { success = false, message = "OrderId không hợp lệ" });
                }

                var order = await _orderRepository.GetByIdAsync(orderId);

                System.Diagnostics.Debug.WriteLine($"Order found: {order != null}");

                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                System.Diagnostics.Debug.WriteLine($"Customer: {order.Customer?.FullName ?? "NULL"}");
                System.Diagnostics.Debug.WriteLine($"OrderStatus: {order.OrderStatus?.StatusName ?? "NULL"}");

                var orderDetails = await _orderDetailsRepository.GetByOrderIdAsync(orderId);

                System.Diagnostics.Debug.WriteLine($"OrderDetails count: {orderDetails?.Count ?? 0}");

                var viewModel = new WEBBANDIENTHOAI.ViewModels.OrderDetailsViewModel
                {
                    OrderId = order.OrderId,
                    OrderDate = order.OrderDate,
                    CustomerName = order.Customer?.FullName ?? "N/A",
                    CustomerPhone = order.Customer?.Phone ?? "N/A",
                    CustomerEmail = order.Customer?.Email ?? "N/A",
                    ShippingAddress = order.ShippingAddress ?? "N/A",
                    PaymentMethod = order.PaymentMethod ?? "COD",
                    StatusId = order.StatusId,
                    StatusName = order.OrderStatus?.StatusName ?? "Không xác định",
                    Notes = order.Notes ?? "",
                    Total = order.Total,
                    DiscountAmount = order.DiscountAmount,
                    PointsUsed = order.PointsUsed,
                    AppliedVoucherCode = order.Voucher?.Code,
                    OrderDetails = orderDetails?.Select(od => new WEBBANDIENTHOAI.ViewModels.OrderDetailItem
                    {
                        OrderDetailId = od.OrderDetailId,
                        ProductId = od.ProductId,
                        ProductName = od.Product?.Name ?? "N/A",
                        ProductImage = od.Product?.PrimaryImage != null
                            ? Url.Action("GetProductImage", "HomeUser", new { imageId = od.Product.PrimaryImage.ImageId })
                            : "/images/default-product.png",
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice
                    }).ToList() ?? new List<WEBBANDIENTHOAI.ViewModels.OrderDetailItem>()
                };

                System.Diagnostics.Debug.WriteLine("=== Returning success ===");

                return Json(new { success = true, data = viewModel });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"=== ERROR: {ex.Message} ===");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");

                return Json(new
                {
                    success = false,
                    message = $"Lỗi: {ex.Message}",
                    details = ex.InnerException?.Message ?? ""
                });
            }
        }

        // GET: Admin/Order/Edit/5
        [Route("Admin/Order/Edit/{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "ID không được để trống";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var order = await _orderRepository.GetByIdAsync(id.Value);
                if (order == null)
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng";
                    return RedirectToAction(nameof(Index));
                }

                var statuses = await _orderStatusRepository.GetAllAsync();
                ViewBag.StatusList = statuses;

                return View(order);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Order/Edit/5
        [HttpPost]
        [Route("Admin/Order/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderId,StatusId")] Order order)
        {
            try
            {
                if (id != order.OrderId)
                {
                    TempData["Error"] = "ID không khớp";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _orderRepository.UpdateStatusAsync(order.OrderId, order.StatusId);
                if (result)
                {
                    TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công!";

                    // LOYALTY HOOK
                    try
                    {
                        if (order.StatusId == 4) await _promotionSvc.AwardPointsAsync(order.OrderId);
                        else if (order.StatusId == 5 || order.StatusId == 6) await _promotionSvc.RevokePointsAsync(order.OrderId);
                    }
                    catch { /* ignore */ }
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy đơn hàng để cập nhật";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi cập nhật: {ex.Message}";
                return RedirectToAction(nameof(Edit), new { id });
            }
        }
    }
}