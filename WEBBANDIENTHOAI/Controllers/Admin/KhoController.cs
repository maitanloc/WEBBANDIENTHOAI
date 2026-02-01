using Microsoft.AspNetCore.Mvc;
using WEBBANDIENTHOAI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using WEBBANDIENTHOAI.Repository.Admin;

namespace WEBBANDIENTHOAI.Controllers.Admin
{
    public class KhoController : Controller
    {
        private readonly IKhohangRepository _khohangRepository;

        public KhoController(IKhohangRepository khohangRepository)
        {
            _khohangRepository = khohangRepository;
        }

        public async Task<IActionResult> Index()
        {
            var inventory = await _khohangRepository.GetAllInventoryAsync();

            ViewBag.TotalItems = await _khohangRepository.GetTotalInventoryCountAsync();
            ViewBag.LowStockItems = (await _khohangRepository.GetLowStockItemsAsync()).Count();

            return View("~/Views/Inventory/Khohang.cshtml", inventory);
        }

        [HttpPost]
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return RedirectToAction("Index");
            }

            var results = await _khohangRepository.SearchInventoryAsync(searchTerm);

            ViewBag.TotalItems = results.Count();
            ViewBag.LowStockItems = results.Count(i => i.CurrentQuantity <= i.MinimumQuantity);
            ViewBag.SearchTerm = searchTerm;

            return View("~/Views/Inventory/Khohang.cshtml", results);
        }

        public async Task<IActionResult> Details(int id)
        {
            var inventory = await _khohangRepository.GetInventoryByIdAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }
            return View(inventory);
        }

        public async Task<IActionResult> LowStock()
        {
            var lowStockItems = await _khohangRepository.GetLowStockItemsAsync();

            ViewBag.TotalItems = lowStockItems.Count();
            ViewBag.LowStockItems = lowStockItems.Count();
            ViewBag.IsLowStockView = true;

            return View("~/Views/Inventory/Khohang.cshtml", lowStockItems);
        }

        // ===============================================
        // ACTION XUẤT EXCEL (ĐÃ SỬA CHO EPPLUS 4.x)
        // ===============================================
        [HttpGet]
        public async Task<IActionResult> ExportExcel()
        {
            // Dòng thiết lập LicenseContext đã được loại bỏ vì không cần thiết
            // với phiên bản EPPlus < 5.0.

            // Lấy toàn bộ dữ liệu kho
            var data = await _khohangRepository.GetAllInventoryAsync();

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("BaoCaoKho");

                // ======= TIÊU ĐỀ =======
                ws.Cells["A1"].Value = "BÁO CÁO KHO HÀNG";
                ws.Cells["A1"].Style.Font.Bold = true;
                ws.Cells["A1"].Style.Font.Size = 18;

                // ======= HEADER =======
                ws.Cells["A3"].Value = "Mã kho";
                ws.Cells["B3"].Value = "Sản phẩm";
                ws.Cells["C3"].Value = "Mã sản phẩm (ID)";
                ws.Cells["D3"].Value = "Tồn hiện tại";
                ws.Cells["E3"].Value = "Tồn tối thiểu";
                ws.Cells["F3"].Value = "Tồn tối đa";
                ws.Cells["G3"].Value = "Vị trí";
                ws.Cells["H3"].Value = "Cập nhật cuối";
                ws.Cells["I3"].Value = "Trạng thái";

                using (var range = ws.Cells["A3:I3"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.Orange);
                    range.Style.Font.Color.SetColor(Color.White);
                }

                // ======= DATA =======
                int row = 4;
                foreach (var item in data)
                {
                    ws.Cells[row, 1].Value = item.StockCode;
                    // Đã dùng .Name thay vì .ProductName
                    ws.Cells[row, 2].Value = item.Product?.Name ?? "(Chưa gắn sản phẩm)";
                    ws.Cells[row, 3].Value = item.ProductId ?? 0;
                    ws.Cells[row, 4].Value = item.CurrentQuantity;
                    ws.Cells[row, 5].Value = item.MinimumQuantity;
                    ws.Cells[row, 6].Value = item.MaximumQuantity;
                    ws.Cells[row, 7].Value = item.Location;
                    ws.Cells[row, 8].Value = item.LastUpdated.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

                    ws.Cells[row, 9].Value = item.CurrentQuantity <= item.MinimumQuantity
                        ? "Sắp hết hàng"
                        : "Còn hàng";

                    // Tô màu trạng thái
                    if (item.CurrentQuantity <= item.MinimumQuantity)
                        ws.Cells[row, 9].Style.Font.Color.SetColor(Color.Red);
                    else
                        ws.Cells[row, 9].Style.Font.Color.SetColor(Color.Green);

                    row++;
                }

                ws.Cells["A:Z"].AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string fileName = $"BaoCaoKho_{DateTime.Now:yyyyMMdd}.xlsx";
                return File(
                    stream,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    fileName
                );
            }
        }
    }
}