using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.ViewModels;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<ProductListItemVm>> GetPagedAsync(ProductQueryFilter filter, int page = 1, int pageSize = 20)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            IQueryable<Product> q = _context.Products.AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.ProductStatus)
                .Include(p => p.PrimaryImage);

            // Apply filters
            if (filter.CategoryId.HasValue)
                q = q.Where(p => p.CategoryId == filter.CategoryId.Value);

            if (!string.IsNullOrWhiteSpace(filter.Brand))
            {
                var b = filter.Brand.Trim();
                q = q.Where(p => p.Brand != null && p.Brand.Contains(b));
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var s = filter.Search.Trim();
                q = q.Where(p =>
                    p.Name.Contains(s) ||
                    p.SKU.Contains(s) ||
                    p.StockCode.Contains(s));
            }

            if (filter.MinPrice.HasValue)
                q = q.Where(p => p.Price >= filter.MinPrice.Value);

            if (filter.MaxPrice.HasValue)
                q = q.Where(p => p.Price <= filter.MaxPrice.Value);

            if (filter.StatusId.HasValue)
                q = q.Where(p => p.StatusId == filter.StatusId.Value);

            // Total count
            var total = await q.CountAsync();

            // Sorting
            q = filter.SortBy?.ToLower() switch
            {
                "price_asc" => q.OrderBy(p => p.Price),
                "price_desc" => q.OrderByDescending(p => p.Price),
                "name_asc" => q.OrderBy(p => p.Name),
                "name_desc" => q.OrderByDescending(p => p.Name),
                "newest" => q.OrderByDescending(p => p.CreatedAt),
                _ => q.OrderBy(p => p.Name)
            };

            // Paging
            var items = await q
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductListItemVm
                {
                    ProductId = p.ProductId,
                    SKU = p.SKU,
                    Name = p.Name,
                    Brand = p.Brand,
                    Price = p.Price,
                    OldPrice = p.OldPrice,
                    StockCode = p.StockCode,
                    ShortDescription = p.ShortDescription,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.CategoryName,
                    StatusId = p.StatusId,
                    StatusName = p.ProductStatus.StatusName,
                    PrimaryImageId = p.PrimaryImage != null ? p.PrimaryImage.ImageId : (int?)null
                })
                .ToListAsync();

            return new PagedResult<ProductListItemVm>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize)
            };
        }

        public async Task<ProductDetailsVm?> GetByIdWithDetailsAsync(int productId)
        {
            var p = await _context.Products.AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.ProductStatus)
                .Include(x => x.Images)
                .Include(x => x.LaptopConfiguration)
                .Include(x => x.PhoneConfiguration)
                .FirstOrDefaultAsync(x => x.ProductId == productId);

            if (p == null) return null;

            return MapToDetailsVm(p);
        }

        public async Task<ProductDetailsVm?> GetBySkuWithDetailsAsync(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku)) return null;

            var p = await _context.Products.AsNoTracking()
                .Include(x => x.Category)
                .Include(x => x.ProductStatus)
                .Include(x => x.Images)
                .Include(x => x.LaptopConfiguration)
                .Include(x => x.PhoneConfiguration)
                .FirstOrDefaultAsync(x => x.SKU == sku);

            if (p == null) return null;

            return MapToDetailsVm(p);
        }

        public async Task<byte[]?> GetImageBytesAsync(int imageId)
        {
            var img = await _context.ProductImages.AsNoTracking()
                .FirstOrDefaultAsync(i => i.ImageId == imageId);
            return img?.ImagePath;
        }

        private ProductDetailsVm MapToDetailsVm(Product p)
        {
            return new ProductDetailsVm
            {
                ProductId = p.ProductId,
                SKU = p.SKU,
                Name = p.Name,
                Brand = p.Brand,
                Price = p.Price,
                OldPrice = p.OldPrice,
                StockCode = p.StockCode,
                Color = p.Color,
                Size = p.Size,
                ShortDescription = p.ShortDescription,
                StatusId = p.StatusId,
                StatusName = p.ProductStatus?.StatusName,
                CategoryId = p.CategoryId,
                CategoryName = p.Category?.CategoryName,
                CreatedAt = p.CreatedAt,
                PrimaryImageId = p.PrimaryImage?.ImageId,
                Images = p.Images?.Select(i => new ProductImageVm { ImageId = i.ImageId }).ToList() ?? new List<ProductImageVm>(),
                LaptopConfiguration = p.LaptopConfiguration == null ? null : new LaptopConfigurationVm
                {
                    CPU = p.LaptopConfiguration.CPU,
                    RAM = p.LaptopConfiguration.RAM,
                    Storage = p.LaptopConfiguration.Storage,
                    Battery = p.LaptopConfiguration.Battery,
                    OperatingSystem = p.LaptopConfiguration.OperatingSystem,
                    ScreenSize = p.LaptopConfiguration.ScreenSize
                },
                PhoneConfiguration = p.PhoneConfiguration == null ? null : new PhoneConfigurationVm
                {
                    CPU = p.PhoneConfiguration.CPU,
                    Cores = p.PhoneConfiguration.Cores,
                    RAM = p.PhoneConfiguration.RAM,
                    InternalStorage = p.PhoneConfiguration.InternalStorage,
                    Battery = p.PhoneConfiguration.Battery,
                    Camera = p.PhoneConfiguration.Camera
                }
            };
        }
    }
}
