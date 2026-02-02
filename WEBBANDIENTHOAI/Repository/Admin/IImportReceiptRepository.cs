using System.Collections.Generic;
using System.Threading.Tasks;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Repository.Admin
{
    public interface IImportReceiptRepository
    {
        Task<IEnumerable<ImportReceipt>> GetAllAsync();
        Task<ImportReceipt> GetByIdAsync(int id);
        Task<ImportReceiptDetail> GetDetailByIdAsync(int id);
        Task<IEnumerable<ImportReceiptDetail>> GetDetailsByReceiptIdAsync(int receiptId);
        Task CreateAsync(ImportReceipt receipt);
        Task UpdateAsync(ImportReceipt receipt);
        Task<IEnumerable<Product>> SearchProductsAsync(string query);
        Task<Product> GetProductByIdAsync(int id);
        Task CreateProductAsync(Product product);
        Task UpdateInventoryAsync(Inventory inventory);
        Task CreateInventoryAsync(Inventory inventory);
        Task<Inventory> GetInventoryByProductIdAsync(int productId);
    }
}
