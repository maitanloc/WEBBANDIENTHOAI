using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.ViewModels;
using System.Linq;

namespace WEBBANDIENTHOAI.Repository.NguoiDung
{
    public interface IOrderHistoryRepository
    {
        Task<OrderHistoryIndexViewModel> GetOrderHistoryAsync(int customerId);
        Task<OrderHistoryViewModel> GetOrderDetailsAsync(int orderId, int customerId);
    }

    public class OrderHistoryRepository : IOrderHistoryRepository
    {
        private readonly AppDbContext _context;

        public OrderHistoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<OrderHistoryIndexViewModel> GetOrderHistoryAsync(int customerId)
        {
            var orders = await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderHistoryViewModel
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.Total,
                    Status = o.Status,
                    PaymentMethod = o.PaymentMethod,
                    ShippingAddress = o.ShippingAddress,
                    Notes = o.Notes,
                    CustomerName = o.Customer.FullName,
                    CustomerEmail = o.Customer.Email,
                    CustomerPhone = o.Customer.Phone,
                    Items = o.OrderDetails.Select(od => new OrderHistoryItemViewModel
                    {
                        ProductName = od.Product.Name,
                        ProductImage = od.Product.ImageId.HasValue
                            ? $"/Image/ProductImage/{od.Product.ImageId.Value}"
                            : "/Images/default.jpg",
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice
                    }).ToList()
                })
                .ToListAsync();

            return new OrderHistoryIndexViewModel
            {
                Orders = orders
            };
        }

        public async Task<OrderHistoryViewModel> GetOrderDetailsAsync(int orderId, int customerId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Where(o => o.OrderId == orderId && o.CustomerId == customerId)
                .Select(o => new OrderHistoryViewModel
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.Total,
                    Status = o.Status,
                    PaymentMethod = o.PaymentMethod,
                    ShippingAddress = o.ShippingAddress,
                    Notes = o.Notes,
                    Items = o.OrderDetails.Select(od => new OrderHistoryItemViewModel
                    {
                        ProductName = od.Product.Name,
                        ProductImage = od.Product.ImageId.HasValue
                            ? $"/Image/ProductImage/{od.Product.ImageId.Value}"
                            : "/Images/default.jpg",
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            return order;
        }
    }
}