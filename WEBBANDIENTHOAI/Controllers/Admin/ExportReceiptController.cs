using Microsoft.AspNetCore.Mvc;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Repository.Admin;
using System.Threading.Tasks;

namespace WEBBANDIENTHOAI.Controllers.Admin
{
    public class ExportReceiptController : Controller
    {
        private readonly IExportReceiptRepository _exportReceiptRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderDetailsRepository _orderDetailsRepository;

        public ExportReceiptController(
            IExportReceiptRepository exportReceiptRepository,
            IOrderRepository orderRepository,
            IOrderDetailsRepository orderDetailsRepository)
        {
            _exportReceiptRepository = exportReceiptRepository;
            _orderRepository = orderRepository;
            _orderDetailsRepository = orderDetailsRepository;
        }

        // GET: Admin/ExportReceipt
        [Route("Admin/ExportReceipt")]
        [Route("Admin/ExportReceipt/Index")]
        public async Task<IActionResult> Index(
            string search = "",
            string fromDate = "",
            string toDate = "",
            string sortBy = "newest",
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                var result = await _exportReceiptRepository.GetAllAsync(
                    search, fromDate, toDate, sortBy, page, pageSize);

                ViewBag.Search = search;
                ViewBag.FromDate = fromDate;
                ViewBag.ToDate = toDate;
                ViewBag.SortBy = sortBy;
                ViewBag.CurrentPage = page;

                return View(result);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return View(new WEBBANDIENTHOAI.Repository.Admin.PagedResult<ExportReceipt>());
            }
        }

        // GET: Admin/ExportReceipt/Details/5
        [Route("Admin/ExportReceipt/Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var exportReceipt = await _exportReceiptRepository.GetByIdAsync(id);
                if (exportReceipt == null)
                {
                    TempData["Error"] = "Không tìm thấy phiếu xuất";
                    return RedirectToAction(nameof(Index));
                }

                return View(exportReceipt);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/ExportReceipt/GetOrderForExport - AJAX endpoint
        [HttpGet]
        [Route("Admin/ExportReceipt/GetOrderForExport")]
        public async Task<IActionResult> GetOrderForExport(int orderId)
        {
            try
            {
                // Kiểm tra đơn hàng có thể xuất không
                var canExport = await _exportReceiptRepository.CanExportForOrderAsync(orderId);
                if (!canExport)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Đơn hàng không thể xuất kho (chưa hoàn thành hoặc đã xuất)"
                    });
                }

                // Lấy thông tin đơn hàng
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
                }

                // Lấy chi tiết sản phẩm
                var orderDetails = await _orderDetailsRepository.GetByOrderIdAsync(orderId);

                // Tạo DTO
                var dto = new
                {
                    orderId = order.OrderId,
                    customerId = order.CustomerId,
                    customerName = order.Customer?.FullName ?? "N/A",
                    orderDate = order.OrderDate.ToString("dd/MM/yyyy HH:mm"),
                    total = order.Total,
                    items = orderDetails.Select(od => new
                    {
                        productId = od.ProductId,
                        stockCode = od.Product?.StockCode ?? "",
                        sku = od.Product?.SKU ?? "",
                        name = od.Product?.Name ?? "N/A",
                        brand = od.Product?.Brand ?? "",
                        quantity = od.Quantity,
                        unitPrice = od.UnitPrice,
                        totalPrice = od.Quantity * od.UnitPrice
                    }).ToList()
                };

                return Json(new { success = true, data = dto });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Lỗi: {ex.Message}"
                });
            }
        }

        // POST: Admin/ExportReceipt/CreateFromOrder - AJAX endpoint
        [HttpPost]
        [Route("Admin/ExportReceipt/CreateFromOrder")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromOrder([FromBody] ExportReceiptCreateDto dto)
        {
            try
            {
                // Validate
                if (dto == null || !dto.Items.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Dữ liệu không hợp lệ"
                    });
                }

                // Kiểm tra đơn hàng có thể xuất không
                if (dto.OrderId.HasValue)
                {
                    var canExport = await _exportReceiptRepository.CanExportForOrderAsync(dto.OrderId.Value);
                    if (!canExport)
                    {
                        return Json(new
                        {
                            success = false,
                            message = "Đơn hàng không thể xuất kho"
                        });
                    }
                }

                // Generate mã phiếu xuất tự động
                dto.ReceiptNumber = await _exportReceiptRepository.GenerateReceiptNumberAsync();

                // Gọi stored procedure tạo phiếu xuất
                var result = await _exportReceiptRepository.CreateExportReceiptAsync(dto);

                if (result)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Xuất kho thành công!",
                        receiptNumber = dto.ReceiptNumber
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không thể tạo phiếu xuất"
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Lỗi: {ex.Message}"
                });
            }
        }
    }
}