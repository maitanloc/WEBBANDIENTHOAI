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
                // Truyền dữ liệu filter vào ViewBag
                ViewBag.Search = search;
                ViewBag.CustomerId = customerId;
                ViewBag.PaymentMethod = paymentMethod;
                ViewBag.StatusId = statusId;
                ViewBag.StartDate = !string.IsNullOrEmpty(startDate) ? DateTime.Parse(startDate) : (DateTime?)null;
                ViewBag.EndDate = !string.IsNullOrEmpty(endDate) ? DateTime.Parse(endDate) : (DateTime?)null;
                ViewBag.SortBy = sortBy;
                ViewBag.CurrentPage = page;

                // Lấy danh sách trạng thái cho dropdown
                var statuses = await _orderStatusRepository.GetAllAsync();
                ViewBag.Statuses = statuses;

                // Lấy danh sách khách hàng cho dropdown (nếu cần)
                // Nếu bạn có GetAllCustomersAsync trong CustomerRepository
                // ViewBag.Customers = await _customerRepository.GetAllAsync();
                // Nếu không có, tạm thời để danh sách rỗng
                ViewBag.Customers = new List<Customer>();

                // Lấy danh sách đơn hàng
                var orders = await _orderRepository.GetOrdersAsync(
                    search, paymentMethod, statusId, startDate, endDate, sortBy, page, pageSize);

                // Tính tổng doanh thu của trang hiện tại
                ViewBag.TotalRevenue = orders.Items.Sum(o => o.Total);

                // Tính thống kê (nếu cần hiển thị)
                var statistics = await CalculateStatisticsAsync();
                ViewBag.Statistics = statistics;

                return View(orders);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return View(new WEBBANDIENTHOAI.Repository.Admin.PagedResult<Order>());
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

                // Lấy danh sách trạng thái
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

                // Chỉ cập nhật trạng thái
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

        // Helper method để tính thống kê
        private async Task<OrderStatisticsDto> CalculateStatisticsAsync()
        {
            // Bạn cần implement phương thức này trong OrderRepository
            // Hoặc tính toán trực tiếp ở đây
            var allOrders = await _orderRepository.GetOrdersAsync("", "", null, "", "", "newest", 1, int.MaxValue);

            return new OrderStatisticsDto
            {
                TotalOrders = allOrders.TotalCount,
                TotalRevenue = allOrders.Items.Sum(o => o.Total),
                PendingOrders = allOrders.Items.Count(o => o.StatusId == 1),
                ProcessingOrders = allOrders.Items.Count(o => o.StatusId == 2),
                CompletedOrders = allOrders.Items.Count(o => o.StatusId == 3),
                CancelledOrders = allOrders.Items.Count(o => o.StatusId == 4)
            };
        }
    }
}