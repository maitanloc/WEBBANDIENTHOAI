using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Repository.TaiKhoan
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllUsersAsync();
        Task<List<User>> GetStaffUsersAsync();
        Task<User> GetUserByIdAsync(int id);
        Task<bool> CreateUserAsync(User user);
        Task<bool> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> UserExistsAsync(int id);
        Task<bool> UsernameExistsAsync(string username, int? excludeId = null);
        Task<bool> EmailExistsAsync(string email, int? excludeId = null);
    }

    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<List<User>> GetStaffUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => u.RoleId != 3) // Loại trừ Customer
                .OrderBy(u => u.FullName)
                .ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<bool> CreateUserAsync(User user)
        {
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            try
            {
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null) return false;

                // Kiểm tra nếu là admin hệ thống (RoleId = 1) thì không cho xóa
                if (user.RoleId == 1)
                {
                    return false;
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UserExistsAsync(int id)
        {
            return await _context.Users.AnyAsync(u => u.UserId == id);
        }

        public async Task<bool> UsernameExistsAsync(string username, int? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return await _context.Users
                    .AnyAsync(u => u.Username == username && u.UserId != excludeId.Value);
            }
            return await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return await _context.Users
                    .AnyAsync(u => u.Email == email && u.UserId != excludeId.Value);
            }
            return await _context.Users.AnyAsync(u => u.Email == email);
        }
    }
}