using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.ViewModels;
using WEBBANDIENTHOAI.Helpers;
using WEBBANDIENTHOAI.Repository.Admin;

namespace WEBBANDIENTHOAI.Controllers.Admin
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
                // SỬA: Sử dụng repository thay vì context trực tiếp để đảm bảo consistency
                var product = await _productRepo.GetByIdWithIncludesAsync(id.Value);

                if (product == null)
                {
                    return NotFound();
                }

                // Check if product can be edited - SỬA: Sử dụng logic từ repository
                var canEdit = await CanEditProductAsync(product.ProductId);
                ViewData["CanEdit"] = canEdit; // SỬA: Dùng ViewData thay vì ViewBag để test dễ dàng hơn

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
                // SỬA: Sử dụng repository để đảm bảo consistency
                var product = await _productRepo.GetByIdWithIncludesAsync(id.Value);

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

                // Pass edit restrictions to view - SỬA: Dùng ViewData thay vì ViewBag
                ViewData["CanEditProduct"] = true;
                ViewData["CanChangeKeys"] = await CanChangeProductKeysAsync(product.ProductId);

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading product edit for ID {ProductId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải trang sửa sản phẩm";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Name,SKU,Brand,Price,OldPrice,StockCode,Color,Size,ShortDescription,StatusId,CategoryId")] Product product, IFormFile? primaryImage)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await PopulateViewData();
                ViewData["CanChangeKeys"] = await CanChangeProductKeysAsync(id);
                return View(product);
            }

            if (!await CanEditProductAsync(id))
            {
                TempData["Error"] = "Sản phẩm không thể sửa do đã có lịch sử nhập/xuất hoặc đang trong quá trình nhập hàng";
                return RedirectToAction(nameof(Details), new { id });
            }

            // Sử dụng transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var existingProduct = await _context.Products
                    .Include(p => p.PrimaryImage)
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
                    ViewData["CanChangeKeys"] = false;
                    return View(product);
                }

                // Update basic product info
                await _context.Database.ExecuteSqlRawAsync(@"
            UPDATE Products 
            SET Name = {0}, Brand = {1}, Price = {2}, OldPrice = {3}, 
                Color = {4}, Size = {5}, ShortDescription = {6}, 
                StatusId = {7}, CategoryId = {8}
            WHERE ProductId = {9}",
                    product.Name, product.Brand ?? "", product.Price, product.OldPrice,
                    product.Color ?? "", product.Size ?? "", product.ShortDescription ?? "",
                    product.StatusId, product.CategoryId, id);

                // Chỉ cho phép thay đổi SKU/StockCode nếu sản phẩm chưa có lịch sử
                if (await CanChangeProductKeysAsync(id))
                {
                    await _context.Database.ExecuteSqlRawAsync(@"
                UPDATE Products 
                SET SKU = {0}, StockCode = {1} 
                WHERE ProductId = {2}",
                        product.SKU, product.StockCode, id);
                }

                // Xử lý configuration
                await UpdateProductConfigurations(existingProduct);

                // Handle image upload - QUAN TRỌNG: xử lý ảnh sau cùng
                if (primaryImage != null && primaryImage.Length > 0)
                {
                    await HandleImageUpload(existingProduct, primaryImage);
                }

                // Commit transaction
                await transaction.CommitAsync();

                TempData["Success"] = "Cập nhật sản phẩm thành công";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật sản phẩm: " + ex.Message;
            }

            await PopulateViewData();
            ViewData["CanChangeKeys"] = await CanChangeProductKeysAsync(id);
            return View(product);
        }

        // GET: Products/Image/5
        public async Task<IActionResult> Image(int id)
        {
            try
            {
                // Sử dụng raw SQL để lấy ảnh
                var image = await _context.ProductImages
                    .FromSqlRaw("SELECT * FROM ProductImages WHERE ImageId = {0}", id)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();

                if (image?.ImagePath == null || image.ImagePath.Length == 0)
                {
                    return NotFound();
                }

                // Sử dụng ImageHelper để xác định content type
                var contentType = ImageHelper.GetContentType(image.ImagePath);

                return File(image.ImagePath, contentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading image {ImageId}", id);
                return NotFound();
            }
        }

        // ==================== HELPER METHODS ====================

        private async Task<bool> CanEditProductAsync(int productId)
        {
            try
            {
                // SỬA: Sử dụng repository method thay vì query trực tiếp để đảm bảo consistency
                var product = await _productRepo.GetByIdWithIncludesAsync(productId);
                if (product == null) return false;

                // Sử dụng logic từ repository
                var hasInventory = product.Inventory?.Any() == true;
                var hasExportHistory = product.ExportDetails?.Any() == true;

                var hasPendingImport = product.ImportDetails?.Any(ird =>
                    ird.ImportReceipt?.IsFinalized == false) == true;

                return hasInventory && !hasExportHistory && !hasPendingImport;
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> CanChangeProductKeysAsync(int productId)
        {
            try
            {
                // SỬA: Sử dụng repository để lấy product với đầy đủ thông tin
                var product = await _productRepo.GetByIdWithIncludesAsync(productId);
                if (product == null) return false;

                var hasImportHistory = product.ImportDetails?.Any() == true;
                var hasExportHistory = product.ExportDetails?.Any() == true;
                var hasInventory = product.Inventory?.Any() == true;

                return !hasImportHistory && !hasExportHistory && !hasInventory;
            }
            catch
            {
                return false;
            }
        }

        private async Task PopulateViewData()
        {
            try
            {
                var statuses = await _productRepo.GetAllStatusesAsync();
                var categories = await _productRepo.GetAllCategoriesAsync();

                ViewData["StatusId"] = new SelectList(statuses, "StatusId", "StatusName");
                ViewData["CategoryId"] = new SelectList(categories, "CategoryId", "CategoryName");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating view data");
                // Set empty lists để tránh lỗi null
                ViewData["StatusId"] = new SelectList(Enumerable.Empty<ProductStatus>(), "StatusId", "StatusName");
                ViewData["CategoryId"] = new SelectList(Enumerable.Empty<Category>(), "CategoryId", "CategoryName");
            }
        }

        // SỬA: Tách method xử lý configuration để code clean hơn
        private async Task UpdateProductConfigurations(Product existingProduct)
        {
            // Update phone configuration - chỉ cập nhật nếu tồn tại
            if (existingProduct.PhoneConfiguration != null)
            {
                existingProduct.PhoneConfiguration.CPU = Request.Form["PhoneConfiguration.CPU"].FirstOrDefault() ?? existingProduct.PhoneConfiguration.CPU;
                existingProduct.PhoneConfiguration.RAM = Request.Form["PhoneConfiguration.RAM"].FirstOrDefault() ?? existingProduct.PhoneConfiguration.RAM;
                existingProduct.PhoneConfiguration.InternalStorage = Request.Form["PhoneConfiguration.InternalStorage"].FirstOrDefault() ?? existingProduct.PhoneConfiguration.InternalStorage;
                existingProduct.PhoneConfiguration.Battery = Request.Form["PhoneConfiguration.Battery"].FirstOrDefault() ?? existingProduct.PhoneConfiguration.Battery;
                existingProduct.PhoneConfiguration.OperatingSystem = Request.Form["PhoneConfiguration.OperatingSystem"].FirstOrDefault() ?? existingProduct.PhoneConfiguration.OperatingSystem;
                existingProduct.PhoneConfiguration.Screen = Request.Form["PhoneConfiguration.Screen"].FirstOrDefault() ?? existingProduct.PhoneConfiguration.Screen;
                existingProduct.PhoneConfiguration.ScreenTechnology = Request.Form["PhoneConfiguration.ScreenTechnology"].FirstOrDefault() ?? existingProduct.PhoneConfiguration.ScreenTechnology;
                existingProduct.PhoneConfiguration.Resolution = Request.Form["PhoneConfiguration.Resolution"].FirstOrDefault() ?? existingProduct.PhoneConfiguration.Resolution;
                existingProduct.PhoneConfiguration.Camera = Request.Form["PhoneConfiguration.Camera"].FirstOrDefault() ?? existingProduct.PhoneConfiguration.Camera;
                existingProduct.PhoneConfiguration.Ports = Request.Form["PhoneConfiguration.Ports"].FirstOrDefault() ?? existingProduct.PhoneConfiguration.Ports;
                existingProduct.PhoneConfiguration.Color = Request.Form["PhoneConfiguration.Color"].FirstOrDefault() ?? existingProduct.PhoneConfiguration.Color;
            }

            // Update laptop configuration - chỉ cập nhật nếu tồn tại
            if (existingProduct.LaptopConfiguration != null)
            {
                existingProduct.LaptopConfiguration.CPU = Request.Form["LaptopConfiguration.CPU"].FirstOrDefault() ?? existingProduct.LaptopConfiguration.CPU;
                existingProduct.LaptopConfiguration.RAM = Request.Form["LaptopConfiguration.RAM"].FirstOrDefault() ?? existingProduct.LaptopConfiguration.RAM;
                existingProduct.LaptopConfiguration.Storage = Request.Form["LaptopConfiguration.Storage"].FirstOrDefault() ?? existingProduct.LaptopConfiguration.Storage;
                existingProduct.LaptopConfiguration.GraphicsCard = Request.Form["LaptopConfiguration.GraphicsCard"].FirstOrDefault() ?? existingProduct.LaptopConfiguration.GraphicsCard;
                existingProduct.LaptopConfiguration.Battery = Request.Form["LaptopConfiguration.Battery"].FirstOrDefault() ?? existingProduct.LaptopConfiguration.Battery;
                existingProduct.LaptopConfiguration.OperatingSystem = Request.Form["LaptopConfiguration.OperatingSystem"].FirstOrDefault() ?? existingProduct.LaptopConfiguration.OperatingSystem;
                existingProduct.LaptopConfiguration.ScreenSize = Request.Form["LaptopConfiguration.ScreenSize"].FirstOrDefault() ?? existingProduct.LaptopConfiguration.ScreenSize;
                existingProduct.LaptopConfiguration.ScreenTechnology = Request.Form["LaptopConfiguration.ScreenTechnology"].FirstOrDefault() ?? existingProduct.LaptopConfiguration.ScreenTechnology;
                existingProduct.LaptopConfiguration.Resolution = Request.Form["LaptopConfiguration.Resolution"].FirstOrDefault() ?? existingProduct.LaptopConfiguration.Resolution;
                existingProduct.LaptopConfiguration.Ports = Request.Form["LaptopConfiguration.Ports"].FirstOrDefault() ?? existingProduct.LaptopConfiguration.Ports;
                existingProduct.LaptopConfiguration.Color = Request.Form["LaptopConfiguration.Color"].FirstOrDefault() ?? existingProduct.LaptopConfiguration.Color;
                existingProduct.LaptopConfiguration.Weight = Request.Form["LaptopConfiguration.Weight"].FirstOrDefault() ?? existingProduct.LaptopConfiguration.Weight;
            }
        }

        private async Task HandleImageUpload(Product product, IFormFile imageFile)
        {
            try
            {
                // Kiểm tra kích thước và định dạng file (giữ nguyên)
                if (imageFile.Length > 3 * 1024 * 1024)
                {
                    throw new InvalidOperationException("Kích thước ảnh không được vượt quá 3MB");
                }

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

                // Nếu đã có ảnh, UPDATE ảnh hiện có thay vì xóa và tạo mới
                if (product.ImageId.HasValue)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE ProductImages SET ImagePath = {0}, CreatedAt = {1} WHERE ImageId = {2}",
                        imageBytes, DateTime.UtcNow, product.ImageId.Value);

                    // Không cần thay đổi ImageId của product
                }
                else
                {
                    // Nếu chưa có ảnh, tạo mới
                    await _context.Database.ExecuteSqlRawAsync(
                        @"INSERT INTO ProductImages (ProductId, ImagePath, IsPrimary, CreatedAt) 
                  VALUES ({0}, {1}, {2}, {3})",
                        product.ProductId, imageBytes, true, DateTime.UtcNow);

                    // Lấy ImageId mới và cập nhật cho product
                    var newImageId = await _context.ProductImages
                        .Where(img => img.ProductId == product.ProductId && img.IsPrimary)
                        .OrderByDescending(img => img.CreatedAt)
                        .Select(img => img.ImageId)
                        .FirstOrDefaultAsync();

                    await _context.Database.ExecuteSqlRawAsync(
                        "UPDATE Products SET ImageId = {0} WHERE ProductId = {1}",
                        newImageId, product.ProductId);

                    product.ImageId = newImageId;
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