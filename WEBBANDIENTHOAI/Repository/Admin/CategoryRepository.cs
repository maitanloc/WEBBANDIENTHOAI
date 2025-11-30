using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEBBANDIENTHOAI.Repository.Admin
{
    // INTERFACE
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllWithProductsAsync();
        Task<Category> GetByIdAsync(int id);
        Task<Category> GetByIdWithProductsAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> HasProductsAsync(int id);
        Task<bool> CreateAsync(Category category);
        Task<bool> UpdateAsync(Category category);
        Task<bool> DeleteAsync(int id);
    }

    // CLASS IMPLEMENTATION
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;

        public CategoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetAllWithProductsAsync()
        {
            return await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);
        }

        public async Task<Category> GetByIdWithProductsAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.CategoryId == id);
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Categories
                .AnyAsync(c => c.CategoryId == id);
        }

        public async Task<bool> HasProductsAsync(int id)
        {
            var category = await GetByIdWithProductsAsync(id);
            return category?.Products?.Any() == true;
        }

        public async Task<bool> CreateAsync(Category category)
        {
            try
            {
                await _context.Categories.AddAsync(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateAsync(Category category)
        {
            try
            {
                _context.Categories.Update(category);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var category = await GetByIdAsync(id);
                if (category != null)
                {
                    _context.Categories.Remove(category);
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