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

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Name,SKU,Brand,Price,OldPrice,StockCode,Color,Size,ShortDescription,StatusId,CategoryId")] Product product, IFormFile? primaryImage)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            // SỬA: Kiểm tra ModelState trước khi check editability
            if (!ModelState.IsValid)
            {
                await PopulateViewData();
                ViewData["CanChangeKeys"] = await CanChangeProductKeysAsync(id);
                return View(product);
            }

            // Check if product can be edited
            if (!await CanEditProductAsync(id))
            {
                TempData["Error"] = "Sản phẩm không thể sửa do đã có lịch sử nhập/xuất hoặc đang trong quá trình nhập hàng";
                return RedirectToAction(nameof(Details), new { id });
            }

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
                    ViewData["CanChangeKeys"] = false;
                    return View(product);
                }

                // Update basic product info
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

                // SỬA: Xử lý configuration một cách an toàn
                await UpdateProductConfigurations(existingProduct);

                // Handle image upload
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

                // SỬA: Dùng repository để update thay vì context trực tiếp
                await _productRepo.UpdateAsync(existingProduct);

                TempData["Success"] = "Cập nhật sản phẩm thành công";
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database error updating product {ProductId}", id);
                TempData["Error"] = "Lỗi cơ sở dữ liệu khi cập nhật sản phẩm";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product {ProductId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật sản phẩm: " + ex.Message;
            }

            // SỬA: Nếu có lỗi, repopulate view data và return view
            await PopulateViewData();
            ViewData["CanChangeKeys"] = await CanChangeProductKeysAsync(id);
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

                // SỬA: Fallback content type nếu không detect được
                var contentType = "image/jpeg"; // Mặc định
                try
                {
                    // Giả sử có ImageHelper, nếu không thì dùng mặc định
                    contentType = WEBBANDIENTHOAI.Helpers.ImageHelper.GetContentType(imageBytes);
                }
                catch
                {
                    // Nếu ImageHelper không tồn tại, dùng mặc định
                    contentType = "image/jpeg";
                }

                return File(imageBytes, contentType);
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

                // SỬA: Sử dụng repository để lưu ảnh
                var imageId = await _productRepo.SavePrimaryImageAsync(product.ProductId, imageBytes, imageFile.ContentType);

                // Cập nhật ImageId cho product
                product.ImageId = imageId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling image upload for product {ProductId}", product.ProductId);
                throw new InvalidOperationException($"Lỗi khi xử lý ảnh: {ex.Message}");
            }
        }
    }
}