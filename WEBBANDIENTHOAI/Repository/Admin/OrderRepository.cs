using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using System.Linq.Expressions;

namespace WEBBANDIENTHOAI.Repository.Admin
{
    public interface IOrderRepository
    {
        Task<PagedResult<Order>> GetOrdersAsync(
            string search = "",
            string paymentMethod = "",
            int? statusId = null,
            string fromDate = "",
            string toDate = "",
            string sortBy = "newest",
            int page = 1,
            int pageSize = 10);

        Task<Order> GetByIdAsync(int id);
        Task<bool> UpdateStatusAsync(int orderId, int statusId);
        Task<OrderStatisticsDto> GetStatisticsAsync();
    }

    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Order>> GetOrdersAsync(
            string search = "",
            string paymentMethod = "",
            int? statusId = null,
            string fromDate = "",
            string toDate = "",
            string sortBy = "newest",
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                var query = _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderStatus)
                    .AsQueryable();

                // Tìm kiếm
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(o =>
                        EF.Functions.Like(o.Customer.FullName, $"%{search}%") ||
                        o.OrderId.ToString().Contains(search) ||
                        (o.Customer.Phone != null && o.Customer.Phone.Contains(search)));
                }

                // Lọc theo phương thức thanh toán
                if (!string.IsNullOrEmpty(paymentMethod))
                {
                    query = query.Where(o => o.PaymentMethod == paymentMethod);
                }

                // Lọc theo trạng thái
                if (statusId.HasValue)
                {
                    query = query.Where(o => o.StatusId == statusId.Value);
                }

                // Lọc theo ngày
                if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out var from))
                {
                    query = query.Where(o => o.OrderDate.Date >= from.Date);
                }

                if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out var to))
                {
                    query = query.Where(o => o.OrderDate.Date <= to.Date);
                }

                // Sắp xếp
                switch (sortBy)
                {
                    case "oldest":
                        query = query.OrderBy(o => o.OrderDate);
                        break;
                    case "highest":
                        query = query.OrderByDescending(o => o.Total);
                        break;
                    case "newest":
                    default:
                        query = query.OrderByDescending(o => o.OrderDate);
                        break;
                }

                // Phân trang
                var totalCount = await query.CountAsync();
                var orders = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync();

                return new PagedResult<Order>
                {
                    Items = orders,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách đơn hàng: {ex.Message}");
            }
        }

        public async Task<Order> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderStatus)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.OrderId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy đơn hàng theo ID {id}: {ex.Message}");
            }
        }

        public async Task<bool> UpdateStatusAsync(int orderId, int statusId)
        {
            try
            {
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null)
                    return false;

                // Kiểm tra trạng thái có tồn tại không
                var statusExists = await _context.OrderStatuses.AnyAsync(s => s.StatusId == statusId);
                if (!statusExists)
                    throw new InvalidOperationException($"Trạng thái ID {statusId} không tồn tại");

                order.StatusId = statusId;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật trạng thái đơn hàng: {ex.Message}");
            }
        }

        public async Task<OrderStatisticsDto> GetStatisticsAsync()
        {
            try
            {
                var orders = await _context.Orders.AsNoTracking().ToListAsync();
                var statuses = await _context.OrderStatuses.AsNoTracking().ToListAsync();

                var pendingId = statuses.FirstOrDefault(s => s.StatusName.Contains("Pending") || s.StatusName.Contains("Chờ"))?.StatusId ?? 1;
                var processingId = statuses.FirstOrDefault(s => s.StatusName.Contains("Processing") || s.StatusName.Contains("Đang"))?.StatusId ?? 2;
                var completedId = statuses.FirstOrDefault(s => s.StatusName.Contains("Completed") || s.StatusName.Contains("Hoàn thành"))?.StatusId ?? 3;
                var cancelledId = statuses.FirstOrDefault(s => s.StatusName.Contains("Cancel") || s.StatusName.Contains("Hủy"))?.StatusId ?? 4;

                return new OrderStatisticsDto
                {
                    TotalOrders = orders.Count,
                    TotalRevenue = orders.Sum(o => o.Total),
                    PendingOrders = orders.Count(o => o.StatusId == pendingId),
                    ProcessingOrders = orders.Count(o => o.StatusId == processingId),
                    CompletedOrders = orders.Count(o => o.StatusId == completedId),
                    CancelledOrders = orders.Count(o => o.StatusId == cancelledId)
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tính thống kê đơn hàng: {ex.Message}");
            }
        }
    }

    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}