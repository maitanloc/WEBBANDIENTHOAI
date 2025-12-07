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

        public OrderController(
            IOrderRepository orderRepository,
            IOrderStatusRepository orderStatusRepository,
            ICustomerRepository customerRepository)
        {
            _orderRepository = orderRepository;
            _orderStatusRepository = orderStatusRepository;
            _customerRepository = customerRepository;
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

        // POST: Order/UpdateStatus - AJAX endpoint cho modal popup
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