using Microsoft.AspNetCore.Mvc;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Repository.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEBBANDIENTHOAI.Controllers.Admin
{
    public class ImportReceiptController : Controller
    {
        private readonly IImportReceiptRepository _repository;
        private readonly IKhohangRepository _khohangRepository;

        public ImportReceiptController(IImportReceiptRepository repository, IKhohangRepository khohangRepository)
        {
            _repository = repository;
            _khohangRepository = khohangRepository;
        }

        // Danh sách phiếu nhập hàng
        public async Task<IActionResult> Index()
        {
            try
            {
                var importReceipts = await _repository.GetAllAsync();
                
                var list = importReceipts.ToList();

                var result = new PagedResult<ImportReceipt>
                {
                    Items = list,
                    TotalCount = list.Count,
                    TotalPages = 1,
                    CurrentPage = 1
                };

                ViewBag.CurrentPage = 1;
                ViewBag.TotalCount = result.TotalCount;

                return View("~/Views/ImportReceipt/Index.cshtml", result);
            }
            catch (Exception ex)
            {
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
                var receipt = await _repository.GetByIdAsync(id);

                if (receipt == null)
                {
                    return Json(new { success = false, message = "Phiếu nhập không tồn tại" });
                }

                var details = await _repository.GetDetailsByReceiptIdAsync(id);

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
                
                var products = await _repository.SearchProductsAsync(query);

                return Json(new { success = true, data = products.Select(p => new
                    {
                        productId = p.ProductId,
                        name = p.Name,
                        sku = p.SKU,
                        brand = p.Brand ?? "",
                        price = p.Price
                    }) 
                });
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
                    SupplierName = model.SupplierName,
                    CreatedByUserId = 1, // TODO: Lấy từ User hiện tại
                    Details = new List<ImportReceiptDetail>(),
                    TotalQuantity = 0,
                    TotalValue = 0,
                    IsFinalized = true // Auto finalize for now
                };

                int totalQty = 0;
                decimal totalVal = 0;

                foreach (var item in model.Items)
                {
                    // Update or create product
                    Product product = null;
                    if (item.ProductId.HasValue && item.ProductId.Value > 0)
                    {
                        product = await _repository.GetProductByIdAsync(item.ProductId.Value);
                    }

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
                        await _repository.CreateProductAsync(product);
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
                        
                        totalQty += item.Quantity;
                        totalVal += detailTotalCost;

                        // Cập nhật tồn kho via repository
                        var inventory = await _repository.GetInventoryByProductIdAsync(product.ProductId);

                        if (inventory != null)
                        {
                            inventory.CurrentQuantity += item.Quantity;
                            inventory.LastUpdated = DateTime.Now;
                            await _repository.UpdateInventoryAsync(inventory);
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
                            await _repository.CreateInventoryAsync(newInventory);
                        }
                    }
                }

                importReceipt.TotalQuantity = totalQty;
                importReceipt.TotalValue = totalVal;
                importReceipt.LastUpdated = DateTime.Now;

                await _repository.CreateAsync(importReceipt);

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
