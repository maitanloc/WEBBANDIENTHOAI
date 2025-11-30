using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEBBANDIENTHOAI.Repository.Admin
{
    // INTERFACE
    public interface IProductStatusRepository
    {
        // GET - Truy vấn đọc
        Task<IEnumerable<ProductStatus>> GetAllWithProductsAsync();
        Task<ProductStatus> GetByIdAsync(byte id);
        Task<ProductStatus> GetByIdWithProductsAsync(byte id);
        Task<bool> ExistsAsync(byte id);
        Task<bool> HasProductsAsync(byte id);

        // POST - Truy vấn ghi
        Task<bool> CreateAsync(ProductStatus productStatus);
        Task<bool> UpdateAsync(ProductStatus productStatus);
        Task<bool> DeleteAsync(byte id);
    }

    // CLASS IMPLEMENTATION
    public class ProductStatusRepository : IProductStatusRepository
    {
        private readonly AppDbContext _context;

        public ProductStatusRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductStatus>> GetAllWithProductsAsync()
        {
            return await _context.ProductStatuses
                .Include(ps => ps.Products)
                .ToListAsync();
        }

        public async Task<ProductStatus> GetByIdAsync(byte id)
        {
            return await _context.ProductStatuses
                .FirstOrDefaultAsync(ps => ps.StatusId == id);
        }

        public async Task<ProductStatus> GetByIdWithProductsAsync(byte id)
        {
            return await _context.ProductStatuses
                .Include(ps => ps.Products)
                .FirstOrDefaultAsync(ps => ps.StatusId == id);
        }

        public async Task<bool> ExistsAsync(byte id)
        {
            return await _context.ProductStatuses
                .AnyAsync(ps => ps.StatusId == id);
        }

        public async Task<bool> HasProductsAsync(byte id)
        {
            var status = await GetByIdWithProductsAsync(id);
            return status?.Products?.Any() == true;
        }

        public async Task<bool> CreateAsync(ProductStatus productStatus)
        {
            try
            {
                await _context.ProductStatuses.AddAsync(productStatus);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(ProductStatus productStatus)
        {
            try
            {
                _context.ProductStatuses.Update(productStatus);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(byte id)
        {
            try
            {
                var status = await GetByIdAsync(id);
                if (status != null)
                {
                    _context.ProductStatuses.Remove(status);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}