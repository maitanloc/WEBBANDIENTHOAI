using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Repository.TaiKhoan
{
    public interface IRoleRepository
    {
        Task<List<Role>> GetAllRolesAsync();
        Task<Role> GetRoleByIdAsync(int id);
        Task<bool> CreateRoleAsync(Role role);
        Task<bool> UpdateRoleAsync(Role role);
        Task<bool> DeleteRoleAsync(int id);
        Task<bool> RoleExistsAsync(int id);
        Task<bool> RoleNameExistsAsync(string roleName, int? excludeId = null);
    }

    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Roles
                .OrderBy(r => r.RoleName)
                .ToListAsync();
        }

        public async Task<Role> GetRoleByIdAsync(int id)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleId == id);
        }

        public async Task<bool> CreateRoleAsync(Role role)
        {
            try
            {
                _context.Roles.Add(role);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateRoleAsync(Role role)
        {
            try
            {
                _context.Roles.Update(role);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            try
            {
                var role = await _context.Roles.FindAsync(id);
                if (role == null) return false;

                // Kiểm tra xem có user nào đang sử dụng role này không
                var usersWithRole = await _context.Users
                    .AnyAsync(u => u.RoleId == id);

                if (usersWithRole)
                {
                    return false; // Không thể xóa role đang được sử dụng
                }

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RoleExistsAsync(int id)
        {
            return await _context.Roles.AnyAsync(r => r.RoleId == id);
        }

        public async Task<bool> RoleNameExistsAsync(string roleName, int? excludeId = null)
        {
            if (excludeId.HasValue)
            {
                return await _context.Roles
                    .AnyAsync(r => r.RoleName == roleName && r.RoleId != excludeId.Value);
            }
            return await _context.Roles.AnyAsync(r => r.RoleName == roleName);
        }
    }
}