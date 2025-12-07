using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Repository.Admin
{
    public interface IOrderDetailsRepository
    {
        Task<List<OrderDetail>> GetByOrderIdAsync(int orderId);
    }

    public class OrderDetailsRepository : IOrderDetailsRepository
    {
        private readonly AppDbContext _context;

        public OrderDetailsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<OrderDetail>> GetByOrderIdAsync(int orderId)
        {
            try
            {
                return await _context.OrderDetails
                    .Include(od => od.Product)
                        .ThenInclude(p => p.PrimaryImage)  // Load hình ảnh
                    .Where(od => od.OrderId == orderId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy chi tiết đơn hàng: {ex.Message}");
            }
        }
    }
}