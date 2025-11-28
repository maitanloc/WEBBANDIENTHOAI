using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WebBanDienThoai.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEBBANDIENTHOAI.Repository
{
    public interface IKhohangRepository
    {
        Task<IEnumerable<Inventory>> GetAllInventoryAsync();
        Task<Inventory> GetInventoryByIdAsync(int id);
        Task<IEnumerable<Inventory>> SearchInventoryAsync(string searchTerm);
        Task UpdateInventoryAsync(Inventory inventory);
        Task<int> GetTotalInventoryCountAsync();
        Task<IEnumerable<Inventory>> GetLowStockItemsAsync();
    }

    public class KhohangRepository : IKhohangRepository
    {
        private readonly AppDbContext _context;

        public KhohangRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Inventory>> GetAllInventoryAsync()
        {
            return await _context.Inventories
                .Include(i => i.Product)
                .OrderByDescending(i => i.LastUpdated)
                .ToListAsync();
        }

        public async Task<Inventory> GetInventoryByIdAsync(int id)
        {
            return await _context.Inventories
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.InventoryId == id);
        }

        public async Task<IEnumerable<Inventory>> SearchInventoryAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllInventoryAsync();

            return await _context.Inventories
                .Include(i => i.Product)
                .Where(i => i.StockCode.Contains(searchTerm) ||
                           i.Location.Contains(searchTerm) ||
                           (i.Product != null && i.Product.Name.Contains(searchTerm)) ||
                           (i.Product != null && i.Product.SKU.Contains(searchTerm)))
                .ToListAsync();
        }

        public async Task UpdateInventoryAsync(Inventory inventory)
        {
            inventory.LastUpdated = System.DateTime.UtcNow;
            _context.Inventories.Update(inventory);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTotalInventoryCountAsync()
        {
            return await _context.Inventories.CountAsync();
        }

        public async Task<IEnumerable<Inventory>> GetLowStockItemsAsync()
        {
            return await _context.Inventories
                .Include(i => i.Product)
                .Where(i => i.CurrentQuantity <= i.MinimumQuantity)
                .OrderBy(i => i.CurrentQuantity)
                .ToListAsync();
        }
    }
}