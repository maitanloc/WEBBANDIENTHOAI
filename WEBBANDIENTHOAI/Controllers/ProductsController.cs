using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Repository;
using WEBBANDIENTHOAI.ViewModels;

namespace WEBBANDIENTHOAI.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductRepository _productRepo;
        private readonly ILogger<ProductsController> _logger;
        private readonly AppDbContext _context;

        public ProductsController(IProductRepository productRepo, ILogger<ProductsController> logger, AppDbContext context)
        {
            _productRepo = productRepo;
            _logger = logger;
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index(string? search, int? categoryId, byte? statusId,
            decimal? priceMin, decimal? priceMax, bool? hasImage, string? sortBy, int page = 1, int pageSize = 20)
        {
            try
            {
                var (items, total) = await _productRepo.GetFilteredAsync(search, categoryId, statusId, priceMin, priceMax, hasImage, sortBy, page, pageSize);

                ViewData["TotalCount"] = total;

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    Response.Headers["X-Page-Title"] = "Danh sách sản phẩm";
                    return PartialView("_ProductsIndexPartial", items);
                }

                ViewData["Title"] = "Sản phẩm";
                return View(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading products index");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách sản phẩm";
                return View(Enumerable.Empty<ProductListItemVm>());
            }
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var product = await _context.Products
                    .Include(p => p.PrimaryImage)
                    .Include(p => p.ProductStatus)
                    .Include(p => p.Category)
                    .Include(p => p.PhoneConfiguration)
                    .Include(p => p.LaptopConfiguration)
                    .Include(p => p.Inventory)
                    .FirstOrDefaultAsync(m => m.ProductId == id);

                if (product == null)
                {
                    return NotFound();
                }

                // Check if product can be edited
                ViewBag.CanEdit = await CanEditProductAsync(product.ProductId);

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product details for ID {ProductId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải chi tiết sản phẩm";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var product = await _context.Products
                    .Include(p => p.PrimaryImage)
                    .Include(p => p.PhoneConfiguration)
                    .Include(p => p.LaptopConfiguration)
                    .Include(p => p.Inventory)
                    .FirstOrDefaultAsync(p => p.ProductId == id);

                if (product == null)
                {
                    return NotFound();
                }

                // Check if product can be edited
                if (!await CanEditProductAsync(product.ProductId))
                {
                    TempData["Error"] = "Sản phẩm không thể sửa do đã có lịch sử nhập/xuất hoặc đang trong quá trình nhập hàng";
                    return RedirectToAction(nameof(Details), new { id });
                }

                await PopulateViewData();

                // Pass edit restrictions to view
                ViewBag.CanEditProduct = true;
                ViewBag.CanChangeKeys = await CanChangeProductKeysAsync(product.ProductId);

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product edit for ID {ProductId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải trang sửa sản phẩm";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Name,SKU,Brand,Price,OldPrice,StockCode,Color,Size,ShortDescription,StatusId,CategoryId")] Product product, IFormFile? primaryImage)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            // Check if product can be edited
            if (!await CanEditProductAsync(id))
            {
                TempData["Error"] = "Sản phẩm không thể sửa do đã có lịch sử nhập/xuất hoặc đang trong quá trình nhập hàng";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _context.Products
                        .Include(p => p.PrimaryImage)
                        .Include(p => p.PhoneConfiguration)
                        .Include(p => p.LaptopConfiguration)
                        .Include(p => p.Inventory)
                        .FirstOrDefaultAsync(p => p.ProductId == id);

                    if (existingProduct == null)
                    {
                        return NotFound();
                    }

                    // Check if SKU/StockCode is being changed
                    bool skuChanged = existingProduct.SKU != product.SKU;
                    bool stockCodeChanged = existingProduct.StockCode != product.StockCode;

                    if ((skuChanged || stockCodeChanged) && !await CanChangeProductKeysAsync(id))
                    {
                        TempData["Error"] = "Không thể thay đổi SKU/Mã kho vì sản phẩm đã có lịch sử nhập/xuất";
                        await PopulateViewData();
                        ViewBag.CanChangeKeys = false;
                        return View(product);
                    }

                    // Update basic product info - chỉ cho phép sửa các trường không quan trọng
                    existingProduct.Name = product.Name;
                    existingProduct.Brand = product.Brand;
                    existingProduct.Price = product.Price;
                    existingProduct.OldPrice = product.OldPrice;
                    existingProduct.Color = product.Color;
                    existingProduct.Size = product.Size;
                    existingProduct.ShortDescription = product.ShortDescription;
                    existingProduct.StatusId = product.StatusId;
                    existingProduct.CategoryId = product.CategoryId;

                    // Chỉ cho phép thay đổi SKU/StockCode nếu sản phẩm chưa có lịch sử
                    if (await CanChangeProductKeysAsync(id))
                    {
                        existingProduct.SKU = product.SKU;
                        existingProduct.StockCode = product.StockCode;
                    }

                    // Update phone configuration - chỉ cập nhật nếu tồn tại
                    if (existingProduct.PhoneConfiguration != null)
                    {
                        existingProduct.PhoneConfiguration.CPU = Request.Form["PhoneConfiguration.CPU"];
                        existingProduct.PhoneConfiguration.RAM = Request.Form["PhoneConfiguration.RAM"];
                        existingProduct.PhoneConfiguration.InternalStorage = Request.Form["PhoneConfiguration.InternalStorage"];
                        existingProduct.PhoneConfiguration.Battery = Request.Form["PhoneConfiguration.Battery"];
                        existingProduct.PhoneConfiguration.OperatingSystem = Request.Form["PhoneConfiguration.OperatingSystem"];
                        existingProduct.PhoneConfiguration.Screen = Request.Form["PhoneConfiguration.Screen"];
                        existingProduct.PhoneConfiguration.ScreenTechnology = Request.Form["PhoneConfiguration.ScreenTechnology"];
                        existingProduct.PhoneConfiguration.Resolution = Request.Form["PhoneConfiguration.Resolution"];
                        existingProduct.PhoneConfiguration.Camera = Request.Form["PhoneConfiguration.Camera"];
                        existingProduct.PhoneConfiguration.Ports = Request.Form["PhoneConfiguration.Ports"];
                        existingProduct.PhoneConfiguration.Color = Request.Form["PhoneConfiguration.Color"];
                    }

                    // Update laptop configuration - chỉ cập nhật nếu tồn tại
                    if (existingProduct.LaptopConfiguration != null)
                    {
                        existingProduct.LaptopConfiguration.CPU = Request.Form["LaptopConfiguration.CPU"];
                        existingProduct.LaptopConfiguration.RAM = Request.Form["LaptopConfiguration.RAM"];
                        existingProduct.LaptopConfiguration.Storage = Request.Form["LaptopConfiguration.Storage"];
                        existingProduct.LaptopConfiguration.GraphicsCard = Request.Form["LaptopConfiguration.GraphicsCard"];
                        existingProduct.LaptopConfiguration.Battery = Request.Form["LaptopConfiguration.Battery"];
                        existingProduct.LaptopConfiguration.OperatingSystem = Request.Form["LaptopConfiguration.OperatingSystem"];
                        existingProduct.LaptopConfiguration.ScreenSize = Request.Form["LaptopConfiguration.ScreenSize"];
                        existingProduct.LaptopConfiguration.ScreenTechnology = Request.Form["LaptopConfiguration.ScreenTechnology"];
                        existingProduct.LaptopConfiguration.Resolution = Request.Form["LaptopConfiguration.Resolution"];
                        existingProduct.LaptopConfiguration.Ports = Request.Form["LaptopConfiguration.Ports"];
                        existingProduct.LaptopConfiguration.Color = Request.Form["LaptopConfiguration.Color"];
                        existingProduct.LaptopConfiguration.Weight = Request.Form["LaptopConfiguration.Weight"];
                    }

                    // Handle image upload - SỬA LỖI Ở ĐÂY
                    if (primaryImage != null && primaryImage.Length > 0)
                    {
                        await HandleImageUpload(existingProduct, primaryImage);
                    }

                    // Update inventory if stock code changed và được phép
                    if (stockCodeChanged && await CanChangeProductKeysAsync(id))
                    {
                        var inventory = await _context.Inventory.FirstOrDefaultAsync(i => i.ProductId == id);
                        if (inventory != null)
                        {
                            inventory.StockCode = product.StockCode;
                            inventory.LastUpdated = DateTime.UtcNow;
                        }
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật sản phẩm thành công";
                    return RedirectToAction(nameof(Details), new { id });
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogError(dbEx, "Database error updating product {ProductId}", id);
                    TempData["Error"] = "Lỗi cơ sở dữ liệu khi cập nhật sản phẩm: " + dbEx.InnerException?.Message;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating product {ProductId}", id);
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật sản phẩm: " + ex.Message;
                }
            }

            await PopulateViewData();
            ViewBag.CanChangeKeys = await CanChangeProductKeysAsync(id);
            return View(product);
        }

        // GET: Products/Image/5
        public async Task<IActionResult> Image(int id)
        {
            try
            {
                var imageBytes = await _productRepo.GetImageBytesAsync(id);
                if (imageBytes == null || imageBytes.Length == 0)
                {
                    return NotFound();
                }

                // Use ImageHelper for content type detection
                var contentType = WEBBANDIENTHOAI.Helpers.ImageHelper.GetContentType(imageBytes);
                return File(imageBytes, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading image {ImageId}", id);
                return NotFound();
            }
        }

        // Helper methods
        private async Task<bool> CanEditProductAsync(int productId)
        {
            // Sản phẩm có thể sửa khi:
            // - Có tồn kho (đã nhập thành công) VÀ
            // - Không có lịch sử xuất kho (chưa bán) VÀ
            // - Không có phiếu nhập chưa finalized
            var hasInventory = await _context.Inventory.AnyAsync(i => i.ProductId == productId);

            var hasExportHistory = await _context.ExportReceiptDetails.AnyAsync(erd => erd.ProductId == productId);

            var hasPendingImport = await _context.ImportReceiptDetails
                .Where(ird => ird.ProductId == productId)
                .Join(_context.ImportReceipts,
                    ird => ird.ImportReceiptId,
                    ir => ir.ImportReceiptId,
                    (ird, ir) => ir)
                .AnyAsync(ir => !ir.IsFinalized);

            return hasInventory && !hasExportHistory && !hasPendingImport;
        }

        private async Task<bool> CanChangeProductKeysAsync(int productId)
        {
            // Chỉ cho phép thay đổi SKU/StockCode khi sản phẩm chưa có bất kỳ lịch sử nào
            var hasImportHistory = await _context.ImportReceiptDetails.AnyAsync(ird => ird.ProductId == productId);
            var hasExportHistory = await _context.ExportReceiptDetails.AnyAsync(erd => erd.ProductId == productId);
            var hasInventory = await _context.Inventory.AnyAsync(i => i.ProductId == productId);

            return !hasImportHistory && !hasExportHistory && !hasInventory;
        }

        private async Task PopulateViewData()
        {
            ViewData["StatusId"] = new SelectList(await _productRepo.GetAllStatusesAsync(), "StatusId", "StatusName");
            ViewData["CategoryId"] = new SelectList(await _productRepo.GetAllCategoriesAsync(), "CategoryId", "CategoryName");
        }

        // SỬA LẠI PHƯƠNG THỨC XỬ LÝ ẢNH
        private async Task HandleImageUpload(Product product, IFormFile imageFile)
        {
            try
            {
                // Kiểm tra kích thước file
                if (imageFile.Length > 3 * 1024 * 1024)
                {
                    throw new InvalidOperationException("Kích thước ảnh không được vượt quá 3MB");
                }

                // Kiểm tra định dạng file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    throw new InvalidOperationException("Chỉ chấp nhận file ảnh (JPG, JPEG, PNG, GIF, WEBP)");
                }

                // Chuyển đổi file sang byte array
                using var memoryStream = new MemoryStream();
                await imageFile.CopyToAsync(memoryStream);
                var imageBytes = memoryStream.ToArray();

                // Kiểm tra nếu sản phẩm đã có ảnh chính
                if (product.ImageId.HasValue)
                {
                    // Cập nhật ảnh hiện có
                    var existingImage = await _context.ProductImages
                        .FirstOrDefaultAsync(img => img.ImageId == product.ImageId.Value);

                    if (existingImage != null)
                    {
                        existingImage.ImagePath = imageBytes;
                        existingImage.CreatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        // Tạo ảnh mới nếu không tìm thấy ảnh cũ
                        var newImage = new ProductImage
                        {
                            ProductId = product.ProductId,
                            ImagePath = imageBytes,
                            IsPrimary = true,
                            CreatedAt = DateTime.UtcNow
                        };
                        _context.ProductImages.Add(newImage);
                        await _context.SaveChangesAsync();
                        product.ImageId = newImage.ImageId;
                    }
                }
                else
                {
                    // Tạo ảnh mới nếu sản phẩm chưa có ảnh
                    var newImage = new ProductImage
                    {
                        ProductId = product.ProductId,
                        ImagePath = imageBytes,
                        IsPrimary = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.ProductImages.Add(newImage);
                    await _context.SaveChangesAsync();
                    product.ImageId = newImage.ImageId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling image upload for product {ProductId}", product.ProductId);
                throw new InvalidOperationException($"Lỗi khi xử lý ảnh: {ex.Message}");
            }
        }
    }
}