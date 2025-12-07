using Microsoft.AspNetCore.Mvc;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Repository.Admin;
using System.Threading.Tasks;

namespace WEBBANDIENTHOAI.Controllers.Admin
{
    public class OrderStatusController : Controller
    {
        private readonly IOrderStatusRepository _orderStatusRepository;

        public OrderStatusController(IOrderStatusRepository orderStatusRepository)
        {
            _orderStatusRepository = orderStatusRepository;
        }

        // GET: OrderStatus
        public async Task<IActionResult> Index(string search = "")
        {
            try
            {
                ViewBag.Search = search;

                if (!string.IsNullOrEmpty(search))
                {
                    var searchResults = await _orderStatusRepository.GetByStatusNameAsync(search);
                    return View(searchResults);
                }

                var orderStatuses = await _orderStatusRepository.GetAllAsync();
                return View(orderStatuses);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return View(new List<OrderStatus>());
            }
        }

        // GET: OrderStatus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "ID không được để trống";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var orderStatus = await _orderStatusRepository.GetByIdAsync(id.Value);
                if (orderStatus == null)
                {
                    TempData["Error"] = "Không tìm thấy trạng thái đơn hàng";
                    return RedirectToAction(nameof(Index));
                }

                return View(orderStatus);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: OrderStatus/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: OrderStatus/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StatusName,Description")] OrderStatus orderStatus)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _orderStatusRepository.CreateAsync(orderStatus);
                    TempData["Success"] = "Trạng thái đơn hàng đã được tạo thành công!";
                    return RedirectToAction(nameof(Index));
                }
                return View(orderStatus);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi tạo: {ex.Message}";
                return View(orderStatus);
            }
        }

        // GET: OrderStatus/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "ID không được để trống";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var orderStatus = await _orderStatusRepository.GetByIdAsync(id.Value);
                if (orderStatus == null)
                {
                    TempData["Error"] = "Không tìm thấy trạng thái đơn hàng";
                    return RedirectToAction(nameof(Index));
                }
                return View(orderStatus);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: OrderStatus/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StatusId,StatusName,Description")] OrderStatus orderStatus)
        {
            if (id != orderStatus.StatusId)
            {
                TempData["Error"] = "ID không khớp";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                if (ModelState.IsValid)
                {
                    await _orderStatusRepository.UpdateAsync(orderStatus);
                    TempData["Success"] = "Trạng thái đơn hàng đã được cập nhật thành công!";
                    return RedirectToAction(nameof(Index));
                }
                return View(orderStatus);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi cập nhật: {ex.Message}";
                return View(orderStatus);
            }
        }

        // GET: OrderStatus/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                TempData["Error"] = "ID không được để trống";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var orderStatus = await _orderStatusRepository.GetByIdAsync(id.Value);
                if (orderStatus == null)
                {
                    TempData["Error"] = "Không tìm thấy trạng thái đơn hàng";
                    return RedirectToAction(nameof(Index));
                }

                return View(orderStatus);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: OrderStatus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _orderStatusRepository.DeleteAsync(id);
                if (result)
                {
                    TempData["Success"] = "Trạng thái đơn hàng đã được xóa thành công!";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy trạng thái đơn hàng để xóa!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi xóa: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}