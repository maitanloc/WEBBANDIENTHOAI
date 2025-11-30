using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.ViewModels;

namespace WEBBANDIENTHOAI.Repository.Admin
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductListItemVm>> GetAllForListWithEditableFlagAsync(int limit = 200)
        {
            var products = await _context.Products
                .AsNoTracking()
                .Include(p => p.PrimaryImage)
                .Include(p => p.ProductStatus)
                .Include(p => p.Inventory)
                .Include(p => p.ImportDetails)
                    .ThenInclude(ird => ird.ImportReceipt)
                .Include(p => p.ExportDetails)
                .OrderByDescending(p => p.CreatedAt)
                .Take(limit)
                .ToListAsync();

            var result = products.Select(p => new ProductListItemVm
            {
                ProductId = p.ProductId,
                SKU = p.SKU,
                Name = p.Name,
                Brand = p.Brand,
                Price = p.Price,
                StockCode = p.StockCode,
                StatusId = p.StatusId,
                StatusName = p.ProductStatus?.StatusName,
                PrimaryImageId = p.PrimaryImage?.ImageId,
                IsEditable = CanEditProduct(p)
            });

            return result;
        }

        public async Task<Product?> GetByIdWithIncludesAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Images)
                .Include(p => p.PrimaryImage)
                .Include(p => p.ProductStatus)
                .Include(p => p.Category)
                .Include(p => p.PhoneConfiguration)
                .Include(p => p.LaptopConfiguration)
                .Include(p => p.Inventory)
                .Include(p => p.ImportDetails)
                    .ThenInclude(ird => ird.ImportReceipt)
                .Include(p => p.ExportDetails)
                .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task<ProductListItemVm?> GetListItemByIdAsync(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Include(p => p.PrimaryImage)
                .Include(p => p.ProductStatus)
                .Include(p => p.Inventory)
                .Include(p => p.ImportDetails)
                    .ThenInclude(ird => ird.ImportReceipt)
                .Include(p => p.ExportDetails)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return null;

            return new ProductListItemVm
            {
                ProductId = product.ProductId,
                SKU = product.SKU,
                Name = product.Name,
                Brand = product.Brand,
                Price = product.Price,
                StockCode = product.StockCode,
                StatusId = product.StatusId,
                StatusName = product.ProductStatus?.StatusName,
                PrimaryImageId = product.PrimaryImage?.ImageId,
                IsEditable = CanEditProduct(product)
            };
        }

        public async Task<bool> HasPendingImportAsync(int productId)
        {
            return await _context.ImportReceiptDetails
                .Where(d => d.ProductId == productId)
                .Join(_context.ImportReceipts,
                      d => d.ImportReceiptId,
                      r => r.ImportReceiptId,
                      (d, r) => r)
                .AnyAsync(r => r.IsFinalized == false);
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task<byte[]?> GetImageBytesAsync(int imageId)
        {
            var img = await _context.ProductImages
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.ImageId == imageId);
            return img?.ImagePath;
        }

        public async Task<(IEnumerable<ProductListItemVm> Items, int TotalCount)> GetFilteredAsync(
      string? search,
      int? categoryId,
      byte? statusId,
      decimal? priceMin,
      decimal? priceMax,
      bool? hasImage,
      string? sortBy,
      int page,
      int pageSize)
        {
            // Sửa lỗi: Tách query base để tránh include không cần thiết khi đếm tổng
            var baseQuery = _context.Products.AsNoTracking();

            // Apply filters - chỉ áp dụng trên base query trước
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.Trim();
                baseQuery = baseQuery.Where(p =>
                    p.Name.Contains(searchTerm) ||
                    p.SKU.Contains(searchTerm) ||
                    p.StockCode.Contains(searchTerm));
            }

            if (categoryId.HasValue)
                baseQuery = baseQuery.Where(p => p.CategoryId == categoryId.Value);

            if (statusId.HasValue)
                baseQuery = baseQuery.Where(p => p.StatusId == statusId.Value);

            if (priceMin.HasValue)
                baseQuery = baseQuery.Where(p => p.Price >= priceMin.Value);
            if (priceMax.HasValue)
                baseQuery = baseQuery.Where(p => p.Price <= priceMax.Value);

            if (hasImage.HasValue)
            {
                baseQuery = hasImage.Value
                    ? baseQuery.Where(p => p.ImageId != null)
                    : baseQuery.Where(p => p.ImageId == null);
            }

            // Đếm tổng trước khi include (hiệu năng tốt hơn)
            var total = await baseQuery.CountAsync();

            // Tạo query với includes cho dữ liệu chi tiết
            var detailedQuery = baseQuery
                .Include(p => p.PrimaryImage)
                .Include(p => p.ProductStatus)
                .Include(p => p.Inventory)
                .Include(p => p.ImportDetails)
                    .ThenInclude(ird => ird.ImportReceipt)
                .Include(p => p.ExportDetails);

            // SỬA LỖI: Tạo biến IQueryable riêng cho phần sort
            IQueryable<Product> sortedQuery = sortBy switch
            {
                "price_asc" => detailedQuery.OrderBy(p => p.Price),
                "price_desc" => detailedQuery.OrderByDescending(p => p.Price),
                "name_asc" => detailedQuery.OrderBy(p => p.Name),
                "name_desc" => detailedQuery.OrderByDescending(p => p.Name),
                _ => detailedQuery.OrderByDescending(p => p.CreatedAt)
            };

            // Phân trang
            var items = await sortedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var list = items.Select(p => new ProductListItemVm
            {
                ProductId = p.ProductId,
                SKU = p.SKU,
                Name = p.Name,
                Brand = p.Brand,
                Price = p.Price,
                StockCode = p.StockCode,
                StatusId = p.StatusId,
                StatusName = p.ProductStatus?.StatusName,
                PrimaryImageId = p.PrimaryImage?.ImageId,
                IsEditable = CanEditProduct(p)
            });

            return (list, total);
        }

        public async Task<int> SavePrimaryImageAsync(int productId, byte[] bytes, string? contentType)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                throw new Exception($"Product with ID {productId} not found");

            // Tạo image mới
            var image = new ProductImage
            {
                ProductId = productId,
                ImagePath = bytes,
                IsPrimary = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.ProductImages.AddAsync(image);
            await _context.SaveChangesAsync();

            // Cập nhật ImageId cho product
            product.ImageId = image.ImageId;
            await _context.SaveChangesAsync();

            return image.ImageId;
        }

        public async Task<IEnumerable<ProductStatus>> GetAllStatusesAsync()
        {
            return await _context.ProductStatuses
                .AsNoTracking()
                .OrderBy(s => s.StatusName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        // Helper method to check if product can be edited - SỬA LOGIC QUAN TRỌNG
        private bool CanEditProduct(Product product)
        {
            // FIX: Kiểm tra null trước khi gọi Any()
            var hasInventory = product.Inventory != null && product.Inventory.Any();
            var hasExportHistory = product.ExportDetails != null && product.ExportDetails.Any();

            // FIX: Kiểm tra ImportDetails và ImportReceipt null
            var hasPendingImport = product.ImportDetails != null &&
                product.ImportDetails.Any(ird =>
                    ird.ImportReceipt != null &&
                    ird.ImportReceipt.IsFinalized == false);

            // Điều kiện: Có inventory VÀ không có export history VÀ không có pending import
            return hasInventory && !hasExportHistory && !hasPendingImport;
        }
    }
}