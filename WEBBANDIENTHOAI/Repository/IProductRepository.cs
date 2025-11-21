using System.Threading.Tasks;
using System.Collections.Generic;
using WEBBANDIENTHOAI.ViewModels;

namespace WEBBANDIENTHOAI.Repository
{
    public interface IProductRepository
    {
        /// <summary>
        /// Truy vấn phân trang kèm bộ lọc cơ bản (category, brand, price range, status, search, sort).
        /// Trả về PagedResult chứa danh sách ProductListItemVm.
        /// </summary>
        Task<PagedResult<ProductListItemVm>> GetPagedAsync(ProductQueryFilter filter, int page = 1, int pageSize = 20);

        /// <summary>
        /// Lấy chi tiết sản phẩm (bao gồm PrimaryImage, Images, Phone/Laptop config).
        /// </summary>
        Task<ProductDetailsVm?> GetByIdWithDetailsAsync(int productId);

        /// <summary>
        /// Lấy product theo SKU (optional, có thể dùng cho URL friendly).
        /// </summary>
        Task<ProductDetailsVm?> GetBySkuWithDetailsAsync(string sku);

        /// <summary>
        /// Lấy raw image bytes theo ImageId (ProductImage.ImageId).
        /// </summary>
        Task<byte[]?> GetImageBytesAsync(int imageId);
    }
}
