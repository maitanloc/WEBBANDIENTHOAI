using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace WEBBANDIENTHOAI.Controllers.Admin
{
    [Area("Admin")]
    [Route("Admin/Reports")]
    // [Authorize(Roles = "Admin,Manager")] // Uncomment when Roles are ready
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Dashboard Overview
        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-30);

            var viewModel = new DashboardViewModel
            {
                TotalRevenue = await _context.Orders
                    .Where(o => o.StatusId == 3) // 3 = Completed/Delivered
                    .SumAsync(o => o.Total),
                
                TotalOrders = await _context.Orders.CountAsync(),
                
                TotalProducts = await _context.Products.CountAsync(),
                
                TotalCustomers = await _context.Customers.CountAsync(),
                
                RecentOrders = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderStatus)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync()
            };

            // Get Revenue Chart Data (Last 30 Days)
            var revenueData = await _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.StatusId == 3)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new RevenueChartData
                {
                    Date = g.Key.ToString("dd/MM"),
                    Revenue = g.Sum(x => x.Total),
                    Orders = g.Count()
                })
                .ToListAsync();

            // Fill in missing dates with 0
            var finalChartData = new List<RevenueChartData>();
            for (var day = startDate.Date; day <= endDate.Date; day = day.AddDays(1))
            {
                var existing = revenueData.FirstOrDefault(r => r.Date == day.ToString("dd/MM"));
                finalChartData.Add(existing ?? new RevenueChartData { Date = day.ToString("dd/MM"), Revenue = 0, Orders = 0 });
            }

            viewModel.RevenueChart = finalChartData;

            return View(viewModel);
        }

        // 2. Revenue Report
        [Route("Revenue")]
        public async Task<IActionResult> Revenue(DateTime? startDate, DateTime? endDate)
        {
            var end = endDate ?? DateTime.Now;
            var start = startDate ?? end.AddDays(-30);

            var orders = await _context.Orders
                .Where(o => o.OrderDate >= start && o.OrderDate <= end && o.StatusId == 3)
                .ToListAsync();

            var viewModel = new RevenueReportViewModel
            {
                StartDate = start,
                EndDate = end,
                TotalRevenue = orders.Sum(o => o.Total),
                TotalOrders = orders.Count,
                AverageOrderValue = orders.Any() ? orders.Average(o => o.Total) : 0,
                DailyStats = orders
                    .GroupBy(o => o.OrderDate.Date)
                    .Select(g => new DailyRevenueItem
                    {
                        Date = g.Key,
                        Revenue = g.Sum(x => x.Total),
                        OrderCount = g.Count()
                    })
                    .OrderByDescending(x => x.Date)
                    .ToList()
            };

            return View(viewModel);
        }

        // 3. Products Report
        [Route("Products")]
        public async Task<IActionResult> Products()
        {
            // Best Sellers (Top 10 by quantity sold)
            var bestSellers = await _context.OrderDetails
                .Include(od => od.Order)
                .Where(od => od.Order.StatusId == 3) // Completed orders only
                .GroupBy(od => new { od.ProductId, od.Product.Name, od.Product.SKU })
                .Select(g => new ProductSalesItem
                {
                    ProductId = g.Key.ProductId,
                    Name = g.Key.Name,
                    SKU = g.Key.SKU,
                    UnitsSold = g.Sum(od => od.Quantity),
                    RevenueGenerated = g.Sum(od => od.Quantity * od.UnitPrice)
                })
                .OrderByDescending(x => x.UnitsSold)
                .Take(10)
                .ToListAsync();

            // Low Stock (Below MinimumQuantity) - Joining Product with Inventory table
            // Note: Since we have Inventory table separate, we need to join or query it.
            // As per Product.cs model, Product has ICollection<Inventory>.
            
            var lowStock = await _context.Inventory
                .Include(i => i.Product)
                .Where(i => i.CurrentQuantity <= i.MinimumQuantity)
                .Select(i => new ProductStockItem
                {
                    ProductId = i.ProductId ?? 0,
                    Name = i.Product.Name,
                    StockCode = i.StockCode,
                    CurrentQuantity = i.CurrentQuantity,
                    MinimumQuantity = i.MinimumQuantity,
                    Status = i.CurrentQuantity == 0 ? "Hết hàng" : "Sắp hết"
                })
                .OrderBy(x => x.CurrentQuantity)
                .Take(20)
                .ToListAsync();

            var viewModel = new ProductReportViewModel
            {
                BestSellers = bestSellers,
                LowStockItems = lowStock
            };

            return View(viewModel);
        }

        // 4. Customers Report
        [Route("Customers")]
        public async Task<IActionResult> Customers()
        {
            // Top Spenders
            var topCustomers = await _context.Orders
                .Where(o => o.StatusId == 3)
                .GroupBy(o => o.CustomerId)
                .Select(g => new
                {
                    CustomerId = g.Key,
                    TotalSpent = g.Sum(x => x.Total),
                    OrderCount = g.Count(),
                    LastOrder = g.Max(x => x.OrderDate)
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(20)
                .Join(_context.Customers, 
                      stats => stats.CustomerId, 
                      cust => cust.CustomerId, 
                      (stats, cust) => new CustomerSpendingItem
                      {
                          CustomerId = cust.CustomerId,
                          FullName = cust.FullName,
                          Email = cust.Email,
                          Phone = cust.Phone,
                          TotalSpent = stats.TotalSpent,
                          TotalOrders = stats.OrderCount,
                          LastOrderDate = stats.LastOrder
                      })
                .ToListAsync();

            var viewModel = new CustomerReportViewModel
            {
                TopCustomers = topCustomers,
                TotalCustomers = await _context.Customers.CountAsync(),
                NewCustomersThisMonth = await _context.Customers.CountAsync(c => c.CreatedAt.Month == DateTime.Now.Month && c.CreatedAt.Year == DateTime.Now.Year)
            };

            return View(viewModel);
        }

        // 5. Inventory Report
        [Route("Inventory")]
        public async Task<IActionResult> Inventory()
        {
            var inventoryItems = await _context.Inventory
                .Include(i => i.Product)
                .OrderBy(i => i.StockCode)
                .ToListAsync();

            var aggregatedItems = inventoryItems.Select(i => new InventoryItemDetail
            {
                StockCode = i.StockCode,
                ProductName = i.Product?.Name ?? "N/A",
                Quantity = i.CurrentQuantity,
                UnitPrice = i.Product?.Price ?? 0,
                TotalValue = i.CurrentQuantity * (i.Product?.Price ?? 0),
                Location = i.Location
            }).ToList();

            var viewModel = new InventoryReportViewModel
            {
                TotalItems = aggregatedItems.Count,
                TotalInventoryValue = aggregatedItems.Sum(x => x.TotalValue),
                Items = aggregatedItems
            };

            return View(viewModel);
        }
    }
}
