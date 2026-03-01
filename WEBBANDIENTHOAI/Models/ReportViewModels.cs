using System;
using System.Collections.Generic;

namespace WEBBANDIENTHOAI.Models.ViewModels
{
      // Dashboard Summary
      public class DashboardViewModel
      {
            public decimal TotalRevenue { get; set; }
            public int TotalOrders { get; set; }
            public int TotalProducts { get; set; }
            public int TotalCustomers { get; set; }
            public decimal TodayRevenue { get; set; }
            public decimal MonthRevenue { get; set; }
            public int NewOrderCount { get; set; }
            public int SuccessOrderCount { get; set; }
            public int CancelledOrderCount { get; set; }
            public int TotalAllOrders { get; set; }

            public decimal SuccessRate => TotalAllOrders > 0
                ? Math.Round((decimal)SuccessOrderCount / TotalAllOrders * 100, 1) : 0;
            public decimal CancelRate => TotalAllOrders > 0
                ? Math.Round((decimal)CancelledOrderCount / TotalAllOrders * 100, 1) : 0;
            public List<Order> RecentOrders { get; set; } = new List<Order>();
            public List<RevenueChartData> RevenueChart { get; set; } = new List<RevenueChartData>();
      }

      public class RevenueChartData
      {
            public string Date { get; set; }
            public decimal Revenue { get; set; }
            public int Orders { get; set; }
      }

      // Revenue Report
      public class RevenueReportViewModel
      {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public decimal TotalRevenue { get; set; }
            public int TotalOrders { get; set; }
            public decimal AverageOrderValue { get; set; }
            
            // Mode: "daily" hoặc "monthly"
            public string ChartMode { get; set; } = "daily";

            // Chart data
            public List<string> ChartLabels { get; set; } = new List<string>();
            public List<decimal> ChartValues { get; set; } = new List<decimal>();
            public List<int> ChartOrderCounts { get; set; } = new List<int>();

            public List<DailyRevenueItem> DailyStats { get; set; } = new List<DailyRevenueItem>();
      }

      public class DailyRevenueItem
      {
            public DateTime Date { get; set; }
            public int OrderCount { get; set; }
            public decimal Revenue { get; set; }
      }

      // Product Report
      public class ProductReportViewModel
      {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public List<ProductSalesItem> BestSellers { get; set; } = new List<ProductSalesItem>();
            public List<ProductStockItem> LowStockItems { get; set; } = new List<ProductStockItem>();
      }

      public class ProductSalesItem
      {
            public int ProductId { get; set; }
            public string Name { get; set; }
            public string SKU { get; set; }
            public int UnitsSold { get; set; }
            public decimal RevenueGenerated { get; set; }
      }

      public class ProductStockItem
      {
            public int ProductId { get; set; }
            public string Name { get; set; }
            public string StockCode { get; set; }
            public int CurrentQuantity { get; set; }
            public int MinimumQuantity { get; set; }
            public string Status { get; set; } // "Hết hàng", "Sắp hết", etc.
      }

      // Customer Report
      public class CustomerReportViewModel
      {
            public List<CustomerSpendingItem> TopCustomers { get; set; } = new List<CustomerSpendingItem>();
            public int TotalCustomers { get; set; }
            public int NewCustomersThisMonth { get; set; }
      }

      public class CustomerSpendingItem
      {
            public int CustomerId { get; set; }
            public string FullName { get; set; }
            public string Phone { get; set; }
            public string Email { get; set; }
            public int TotalOrders { get; set; }
            public decimal TotalSpent { get; set; }
            public DateTime LastOrderDate { get; set; }
      }

      // Inventory Report
      public class InventoryReportViewModel
      {
            public decimal TotalInventoryValue { get; set; }
            public int TotalItems { get; set; }
            public List<InventoryItemDetail> Items { get; set; } = new List<InventoryItemDetail>();
      }

      public class InventoryItemDetail
      {
            public string StockCode { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal TotalValue { get; set; } // Quantity * Price
            public string Location { get; set; }
      }
}
