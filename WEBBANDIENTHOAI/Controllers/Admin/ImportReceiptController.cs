using Microsoft.AspNetCore.Mvc;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Repository.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;

namespace WEBBANDIENTHOAI.Controllers.Admin
{
    public class ImportReceiptController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IKhohangRepository _khohangRepository;

        public ImportReceiptController(AppDbContext context, IKhohangRepository khohangRepository)
        {
            _context = context;
            _khohangRepository = khohangRepository;
        }

        // Danh sách phiếu nhập hàng
        public async Task<IActionResult> Index()
        {
            try
            {
                // Load dữ liệu an toàn mà không include Details (vì Details cũ có null)
                var importReceipts = await _context.ImportReceipts
                    .AsNoTracking()
                    .OrderByDescending(x => x.ImportReceiptId)
                    .ToListAsync();

                var result = new PagedResult<ImportReceipt>
                {
                    Items = importReceipts ?? new List<ImportReceipt>(),
                    TotalCount = importReceipts?.Count ?? 0,
                    TotalPages = 1,
                    CurrentPage = 1
                };

                ViewBag.CurrentPage = 1;
                ViewBag.TotalCount = result.TotalCount;

                return View("~/Views/ImportReceipt/Index.cshtml", result);
            }
            catch (Exception ex)
            {
                // Nếu có lỗi, trả về danh sách rỗng
                ViewBag.Error = $"Lỗi tải dữ liệu: {ex.Message}";
                var emptyResult = new PagedResult<ImportReceipt>
                {
                    Items = new List<ImportReceipt>(),
                    TotalCount = 0,
                    TotalPages = 1,
                    CurrentPage = 1
                };
                return View("~/Views/ImportReceipt/Index.cshtml", emptyResult);
            }
        }

        // API: Lấy chi tiết phiếu nhập
        [HttpGet]
        public async Task<IActionResult> GetDetails(int id)
        {
            try
            {
                var receipt = await _context.ImportReceipts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ImportReceiptId == id);

                if (receipt == null)
                {
                    return Json(new { success = false, message = "Phiếu nhập không tồn tại" });
                }

                // Load Details riêng và lọc null
                var details = await _context.ImportReceiptDetails
                    .AsNoTracking()
                    .Where(x => x.ImportReceiptId == id && x.SnapshotName != null && x.SnapshotSKU != null)
                    .ToListAsync();

                var result = new
                {
                    success = true,
                    data = new
                    {
                        receipt.ImportReceiptId,
                        ReceiptNumber = receipt.ReceiptNumber ?? $"PHN{receipt.ImportReceiptId:00000}",
                        receipt.ImportDate,
                        CreatedByName = "Quản trị viên",
                        TotalQuantity = details.Sum(d => d.Quantity),
                        TotalValue = details.Sum(d => d.TotalCost),
                        Details = details.Select(d => new
                        {
                            d.ImportDetailId,
                            ProductName = d.SnapshotName ?? "N/A",
                            d.SnapshotSKU,
                            d.Quantity,
                            UnitPrice = d.UnitCost,
                            TotalPrice = d.TotalCost
                        }).ToList()
                    }
                };

                return Json(result);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // API: Tìm kiếm sản phẩm
        [HttpGet]
        public async Task<IActionResult> SearchProduct(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Json(new { success = false, data = new List<object>() });
                }

                var products = await _context.Products
                    .Where(p => p.Name.Contains(query) || p.SKU.Contains(query))
                    .Take(10)
                    .Select(p => new
                    {
                        productId = p.ProductId,
                        name = p.Name,
                        sku = p.SKU,
                        brand = p.Brand ?? "",
                        price = p.Price
                    })
                    .ToListAsync();

                return Json(new { success = true, data = products });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API: Tạo phiếu nhập hàng
        [HttpPost]
        public async Task<IActionResult> CreateImport([FromBody] CreateImportModel model)
        {
            try
            {
                if (model?.Items == null || !model.Items.Any())
                {
                    return Json(new { success = false, message = "Phiếu nhập phải có ít nhất 1 sản phẩm" });
                }

                var importReceipt = new ImportReceipt
                {
                    ReceiptNumber = $"PHN{DateTime.Now:yyyyMMddHHmmss}",
                    ImportDate = DateTime.Now,
                    SupplierName = model.SupplierName ?? "Không xác định",
                    CreatedByUserId = 1, // TODO: Lấy từ User hiện tại
                    Details = new List<ImportReceiptDetail>()
                };

                foreach (var item in model.Items)
                {
                    // Kiểm tra hoặc tạo sản phẩm
                    var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == item.ProductId);

                    if (product == null && !string.IsNullOrWhiteSpace(item.ProductName))
                    {
                        // Tạo sản phẩm mới
                        product = new Product
                        {
                            CategoryId = 1,
                            Name = item.ProductName,
                            SKU = item.ProductSKU ?? $"SKU{Guid.NewGuid().ToString().Substring(0, 8)}",
                            StockCode = $"STK{Guid.NewGuid().ToString().Substring(0, 8)}",
                            Price = item.UnitPrice,
                            Brand = item.ProductBrand ?? ""
                        };
                        _context.Products.Add(product);
                        await _context.SaveChangesAsync();
                    }

                    if (product != null)
                    {
                        var detailTotalCost = item.Quantity * item.UnitPrice;
                        var detail = new ImportReceiptDetail
                        {
                            ProductId = product.ProductId,
                            SnapshotSKU = product.SKU,
                            SnapshotName = product.Name,
                            SnapshotBrand = product.Brand ?? "",
                            StockCode = product.StockCode,
                            Quantity = item.Quantity,
                            UnitCost = item.UnitPrice,
                            TotalCost = detailTotalCost,
                            CreatedAt = DateTime.Now
                        };
                        importReceipt.Details.Add(detail);

                        // Cập nhật tồn kho
                        var inventory = await _context.Inventories
                            .FirstOrDefaultAsync(i => i.ProductId == product.ProductId);

                        if (inventory != null)
                        {
                            inventory.CurrentQuantity += item.Quantity;
                            inventory.LastUpdated = DateTime.Now;
                        }
                        else
                        {
                            // Tạo tồn kho mới
                            var newInventory = new Inventory
                            {
                                ProductId = product.ProductId,
                                StockCode = product.StockCode,
                                CurrentQuantity = item.Quantity,
                                MinimumQuantity = 10,
                                MaximumQuantity = 1000,
                                Location = "Chưa xác định",
                                LastUpdated = DateTime.Now
                            };
                            _context.Inventories.Add(newInventory);
                        }
                    }
                }

                // Set total values
                var totalQty = importReceipt.Details.Sum(d => d.Quantity);
                var totalValue = importReceipt.Details.Sum(d => d.TotalCost);
                importReceipt.TotalQuantity = totalQty;
                importReceipt.TotalValue = totalValue;
                importReceipt.LastUpdated = DateTime.Now;

                _context.ImportReceipts.Add(importReceipt);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = "Phiếu nhập hàng được tạo thành công",
                    data = new { importReceipt.ImportReceiptId }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }

    // Model tiếp nhận dữ liệu
    public class CreateImportModel
    {
        public string? SupplierName { get; set; }
        public List<ImportItemModel>? Items { get; set; }
    }

    public class ImportItemModel
    {
        public int? ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? ProductSKU { get; set; }
        public string? ProductBrand { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    // PagedResult model (nếu chưa có)
    public class PagedResult<T>
    {
        public List<T>? Items { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }

        public PagedResult()
        {
            Items = new List<T>();
        }
    }
}
