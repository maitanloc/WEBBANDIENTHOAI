using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Repository.Admin
{
    // Interface định nghĩa các phương thức
    public interface IOrderStatusRepository
    {
        Task<IEnumerable<OrderStatus>> GetAllAsync();
        Task<OrderStatus> GetByIdAsync(int id);
        Task<OrderStatus> CreateAsync(OrderStatus orderStatus);
        Task<OrderStatus> UpdateAsync(OrderStatus orderStatus);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<OrderStatus>> GetByStatusNameAsync(string statusName);
    }

    // Implementation thực hiện các phương thức
    public class OrderStatusRepository : IOrderStatusRepository
    {
        private readonly AppDbContext _context;

        public OrderStatusRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderStatus>> GetAllAsync()
        {
            try
            {
                return await _context.OrderStatuses
                    .OrderBy(s => s.StatusId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách trạng thái: {ex.Message}");
            }
        }

        public async Task<OrderStatus> GetByIdAsync(int id)
        {
            try
            {
                return await _context.OrderStatuses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.StatusId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy trạng thái theo ID {id}: {ex.Message}");
            }
        }

        public async Task<OrderStatus> CreateAsync(OrderStatus orderStatus)
        {
            try
            {
                // Kiểm tra tên trạng thái đã tồn tại chưa
                var exists = await _context.OrderStatuses
                    .AnyAsync(s => s.StatusName == orderStatus.StatusName);

                if (exists)
                    throw new InvalidOperationException($"Tên trạng thái '{orderStatus.StatusName}' đã tồn tại");

                _context.OrderStatuses.Add(orderStatus);
                await _context.SaveChangesAsync();
                return orderStatus;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tạo trạng thái: {ex.Message}");
            }
        }

        public async Task<OrderStatus> UpdateAsync(OrderStatus orderStatus)
        {
            try
            {
                // Kiểm tra tên trạng thái đã tồn tại chưa (trừ bản ghi hiện tại)
                var exists = await _context.OrderStatuses
                    .AnyAsync(s => s.StatusName == orderStatus.StatusName && s.StatusId != orderStatus.StatusId);

                if (exists)
                    throw new InvalidOperationException($"Tên trạng thái '{orderStatus.StatusName}' đã tồn tại");

                var existingStatus = await _context.OrderStatuses.FindAsync(orderStatus.StatusId);
                if (existingStatus == null)
                    throw new KeyNotFoundException($"Không tìm thấy trạng thái với ID {orderStatus.StatusId}");

                existingStatus.StatusName = orderStatus.StatusName;
                existingStatus.Description = orderStatus.Description;

                _context.Entry(existingStatus).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return existingStatus;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật trạng thái: {ex.Message}");
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                // Kiểm tra xem có đơn hàng nào đang sử dụng trạng thái này không
                var hasOrders = await _context.Orders.AnyAsync(o => o.StatusId == id);
                if (hasOrders)
                    throw new InvalidOperationException("Không thể xóa trạng thái này vì có đơn hàng đang sử dụng");

                var orderStatus = await _context.OrderStatuses.FindAsync(id);
                if (orderStatus == null)
                    return false;

                _context.OrderStatuses.Remove(orderStatus);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xóa trạng thái: {ex.Message}");
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.OrderStatuses.AnyAsync(e => e.StatusId == id);
        }

        public async Task<IEnumerable<OrderStatus>> GetByStatusNameAsync(string statusName)
        {
            try
            {
                return await _context.OrderStatuses
                    .Where(s => s.StatusName.Contains(statusName))
                    .OrderBy(s => s.StatusId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tìm kiếm trạng thái: {ex.Message}");
            }
        }
    }
}