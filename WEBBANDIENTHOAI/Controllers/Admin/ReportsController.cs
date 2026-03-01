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
            var today = DateTime.Now.Date;
            var tomorrow = today.AddDays(1);
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var nextMonthStart = monthStart.AddMonths(1);
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-30);

            var allOrdersCount = await _context.Orders.CountAsync();
            var successCount = await _context.Orders.CountAsync(o => o.StatusId == 3);
            var cancelledCount = await _context.Orders.CountAsync(o => o.StatusId == 5 || o.StatusId == 4);
            var newOrderCount = await _context.Orders.CountAsync(o => o.StatusId == 1);
            
            var todayRevenue = await _context.Orders
                .Where(o => o.OrderDate >= today && o.OrderDate < tomorrow)
                .SumAsync(o => (decimal?)o.Total) ?? 0;

            var monthRevenue = await _context.Orders
                .Where(o => o.OrderDate >= monthStart && o.OrderDate < nextMonthStart)
                .SumAsync(o => (decimal?)o.Total) ?? 0;

            var viewModel = new DashboardViewModel
            {
                TotalRevenue = await _context.Orders
                    .SumAsync(o => o.Total),
                
                TotalOrders = allOrdersCount,
                TotalProducts = await _context.Products.CountAsync(),
                TotalCustomers = await _context.Customers.CountAsync(),
                TodayRevenue = todayRevenue,
                MonthRevenue = monthRevenue,
                NewOrderCount = newOrderCount,
                SuccessOrderCount = successCount,
                CancelledOrderCount = cancelledCount,
                TotalAllOrders = allOrdersCount,
                
                RecentOrders = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderStatus)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync()
            };

            // Get Revenue Chart Data (Last 30 Days)
            var revenueData = await _context.Orders
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && (o.StatusId != 4 && o.StatusId != 5))
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
        public async Task<IActionResult> Revenue(DateTime? startDate, DateTime? endDate, string mode = "daily")
        {
            var end = endDate ?? DateTime.Now;
            var start = startDate ?? end.AddDays(-30);

            var orders = await _context.Orders
                .Where(o => o.OrderDate >= start.Date && o.OrderDate <= end.Date.AddDays(1).AddTicks(-1) && (o.StatusId != 4 && o.StatusId != 5))
                .ToListAsync();

            var dailyStats = orders
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new DailyRevenueItem
                {
                    Date = g.Key,
                    Revenue = g.Sum(x => x.Total),
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.Date)
                .ToList();

            // Build chart data
            List<string> chartLabels = new();
            List<decimal> chartValues = new();
            List<int> chartOrderCounts = new();

            if (mode == "monthly")
            {
                // Group by month
                var monthly = orders
                    .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .ToList();

                foreach (var g in monthly)
                {
                    chartLabels.Add($"T{g.Key.Month}/{g.Key.Year}");
                    chartValues.Add(g.Sum(x => x.Total));
                    chartOrderCounts.Add(g.Count());
                }
            }
            else
            {
                // Group by day - fill missing dates
                var dailyMap = orders
                    .GroupBy(o => o.OrderDate.Date)
                    .ToDictionary(g => g.Key, g => new { Revenue = g.Sum(x => x.Total), Count = g.Count() });

                for (var day = start.Date; day <= end.Date; day = day.AddDays(1))
                {
                    chartLabels.Add(day.ToString("dd/MM"));
                    if (dailyMap.TryGetValue(day, out var v))
                    {
                        chartValues.Add(v.Revenue);
                        chartOrderCounts.Add(v.Count);
                    }
                    else
                    {
                        chartValues.Add(0);
                        chartOrderCounts.Add(0);
                    }
                }
            }

            var viewModel = new RevenueReportViewModel
            {
                StartDate = start,
                EndDate = end,
                TotalRevenue = orders.Sum(o => o.Total),
                TotalOrders = orders.Count,
                AverageOrderValue = orders.Any() ? orders.Average(o => o.Total) : 0,
                ChartMode = mode,
                DailyStats = dailyStats,
                ChartLabels = chartLabels,
                ChartValues = chartValues,
                ChartOrderCounts = chartOrderCounts
            };

            return View(viewModel);
        }

        // 3. Products Report
        [Route("Products")]
        public async Task<IActionResult> Products(DateTime? startDate, DateTime? endDate)
        {
            var end = endDate ?? DateTime.Now;
            var start = startDate ?? end.AddDays(-30);

            // Best Sellers (Top 10 by quantity sold)
            var bestSellers = await _context.OrderDetails
                .Include(od => od.Order)
                .Where(od => od.Order.OrderDate >= start.Date && od.Order.OrderDate <= end.Date.AddDays(1).AddTicks(-1) 
                          && (od.Order.StatusId != 4 && od.Order.StatusId != 5)) // Not Cancelled/Returned
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
                StartDate = start,
                EndDate = end,
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
