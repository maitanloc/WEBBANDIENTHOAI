using Microsoft.AspNetCore.Mvc;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Repository.Admin;
using WEBBANDIENTHOAI.Repository.TaiKhoan;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace WEBBANDIENTHOAI.Controllers.Admin
{
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderStatusRepository _orderStatusRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IOrderDetailsRepository _orderDetailsRepository;

        public OrderController(
            IOrderRepository orderRepository,
            IOrderStatusRepository orderStatusRepository,
            ICustomerRepository customerRepository,
            IOrderDetailsRepository orderDetailsRepository)
        {
            _orderRepository = orderRepository;
            _orderStatusRepository = orderStatusRepository;
            _customerRepository = customerRepository;
            _orderDetailsRepository = orderDetailsRepository;
        }

        // GET: Order
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

        // POST: Order/UpdateStatus
        [HttpPost]
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
                    TempData["Success"] = "✅ Cập nhật trạng thái đơn hàng thành công!";
                }
                else
                {
                    TempData["Error"] = "❌ Không tìm thấy đơn hàng để cập nhật";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Lỗi: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Order/GetOrderDetails - AJAX endpoint cho popup chi tiết
        [HttpGet]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                var orderDetails = await _orderDetailsRepository.GetByOrderIdAsync(orderId);

                var viewModel = new WEBBANDIENTHOAI.ViewModels.OrderDetailsViewModel
                {
                    OrderId = order.OrderId,
                    OrderDate = order.OrderDate,
                    CustomerName = order.Customer?.FullName ?? "N/A",
                    CustomerPhone = order.Customer?.Phone ?? "N/A",
                    CustomerEmail = order.Customer?.Email ?? "N/A",
                    ShippingAddress = order.ShippingAddress ?? "N/A",
                    PaymentMethod = order.PaymentMethod,
                    StatusName = order.OrderStatus?.StatusName ?? "N/A",
                    Notes = order.Notes ?? "",
                    Total = order.Total,
                    OrderDetails = orderDetails.Select(od => new WEBBANDIENTHOAI.ViewModels.OrderDetailItem
                    {
                        OrderDetailId = od.OrderDetailId,
                        ProductId = od.ProductId,
                        ProductName = od.Product?.Name ?? "N/A",
                        ProductImage = od.Product?.PrimaryImage != null
                            ? Url.Action("GetProductImage", "HomeUser", new { imageId = od.Product.PrimaryImage.ImageId })
                            : "/images/default-product.png",
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice
                    }).ToList()
                };

                return Json(new { success = true, data = viewModel });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // GET: Order/Edit/5
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

        // POST: Order/Edit/5
        [HttpPost]
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