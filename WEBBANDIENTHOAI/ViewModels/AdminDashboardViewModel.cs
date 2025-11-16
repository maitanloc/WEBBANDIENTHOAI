using System;
using System.Collections.Generic;


 namespace WEBBANDIENTHOAI.ViewModels
{ 
    public class AdminDashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public int NewOrdersCount { get; set; }
        public int LowStockCount { get; set; }

        public List<RecentOrderDto> RecentOrders { get; set; } = new List<RecentOrderDto>();

        // Revenue last 7 days
        public List<string> RevenueByDayLabels { get; set; } = new List<string>();
        public List<decimal> RevenueByDay { get; set; } = new List<decimal>();

        // Stock summary (donut chart)
        public List<string> StockLabels { get; set; } = new List<string>();
        public List<int> StockData { get; set; } = new List<int>();
    }

    public class RecentOrderDto
    {
        public string OrderCode { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
        public decimal Total { get; set; }
        public string StaffOrCustomer { get; set; }
    }

    }