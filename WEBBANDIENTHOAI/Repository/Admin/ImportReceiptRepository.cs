using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Repository.Admin
{
    public class ImportReceiptRepository : IImportReceiptRepository
    {
        private readonly AppDbContext _context;

        public ImportReceiptRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ImportReceipt>> GetAllAsync()
        {
            return await _context.ImportReceipts
                .AsNoTracking()
                .OrderByDescending(x => x.ImportReceiptId)
                .ToListAsync();
        }

        public async Task<ImportReceipt> GetByIdAsync(int id)
        {
            return await _context.ImportReceipts
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ImportReceiptId == id);
        }
        
        public async Task<ImportReceiptDetail> GetDetailByIdAsync(int id)
        {
             return await _context.ImportReceiptDetails.FindAsync(id);
        }

        public async Task<IEnumerable<ImportReceiptDetail>> GetDetailsByReceiptIdAsync(int receiptId)
        {
            return await _context.ImportReceiptDetails
                .AsNoTracking()
                .Where(x => x.ImportReceiptId == receiptId)
                .ToListAsync();
        }

        public async Task CreateAsync(ImportReceipt receipt)
        {
            _context.ImportReceipts.Add(receipt);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(ImportReceipt receipt)
        {
            _context.ImportReceipts.Update(receipt);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<Product>();

            return await _context.Products
                .Where(p => p.Name.Contains(query) || p.SKU.Contains(query))
                .Take(10)
                .ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task CreateProductAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }
        
        public async Task UpdateInventoryAsync(Inventory inventory)
        {
            _context.Inventories.Update(inventory);
             await _context.SaveChangesAsync();
        }

        public async Task CreateInventoryAsync(Inventory inventory)
        {
             _context.Inventories.Add(inventory);
             await _context.SaveChangesAsync();
        }

        public async Task<Inventory> GetInventoryByProductIdAsync(int productId)
        {
             return await _context.Inventories.FirstOrDefaultAsync(i => i.ProductId == productId);
        }
    }
}
