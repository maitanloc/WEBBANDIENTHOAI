using System.Collections.Generic;
using System.Threading.Tasks;
using WEBBANDIENTHOAI.ViewModels;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Repository
{
    public interface IProductRepository
    {
        Task<IEnumerable<ProductListItemVm>> GetAllForListWithEditableFlagAsync(int limit = 200);
        Task<Product?> GetByIdWithIncludesAsync(int id);
        Task<ProductListItemVm?> GetListItemByIdAsync(int id);
        Task<bool> HasPendingImportAsync(int productId);
        Task UpdateAsync(Product product); // Đã sửa từ bool thành void
        Task<byte[]?> GetImageBytesAsync(int imageId);

        Task<IEnumerable<ProductStatus>> GetAllStatusesAsync();
        Task<IEnumerable<Category>> GetAllCategoriesAsync();

        Task<(IEnumerable<ProductListItemVm> Items, int TotalCount)> GetFilteredAsync(
            string? search,
            int? categoryId,
            byte? statusId,
            decimal? priceMin,
            decimal? priceMax,
            bool? hasImage,
            string? sortBy,
            int page,
            int pageSize);

        Task<int> SavePrimaryImageAsync(int productId, byte[] bytes, string? contentType);
    }
}