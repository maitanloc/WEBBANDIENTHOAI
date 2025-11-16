using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.ViewModels; // <-- THÊM DÒNG NÀY (ViewModel/DTO)
using System.Linq;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = new AdminDashboardViewModel();

        // Tổng doanh thu (dùng Orders.Total). Nếu Total nullable, cast an toàn.
        model.TotalRevenue = await _context.Orders
            .Where(o => o.Total != null)
            .SumAsync(o => (decimal?)o.Total) ?? 0m;

        // Doanh thu hôm nay (theo timezone server)
        var today = DateTime.Now.Date;
        model.TodayRevenue = await _context.Orders
            .Where(o => o.OrderDate >= today && o.OrderDate < today.AddDays(1))
            .SumAsync(o => (decimal?)o.Total) ?? 0m;

        // Đơn mới -> số đơn Pending
        model.NewOrdersCount = await _context.Orders
            .Where(o => o.Status == "Pending")
            .CountAsync();

        // Low stock: inventory current <= minimum
        // LƯU Ý: nếu DbSet tên khác (Inventories) -> đổi _context.Inventory thành _context.Inventories
        // Ở đây mình giữ _context.Inventory theo code bạn đang dùng.
        model.LowStockCount = await _context.Inventory
            .Where(i => i.CurrentQuantity <= i.MinimumQuantity)
            .CountAsync();

        // Recent orders (top 6)
        var recent = await _context.Orders
            .OrderByDescending(o => o.OrderDate)
            .Take(6)
            .Select(o => new
            {
                o.OrderId,
                o.OrderDate,
                o.Status,
                o.Total,
                CustomerId = (int?)o.CustomerId,
                CreatedByUserId = o.CreatedByUserId
            })
            .ToListAsync();

        // Resolve customer & user names
        var customerIds = recent.Where(r => r.CustomerId.HasValue).Select(r => r.CustomerId.Value).Distinct().ToList();
        var userIds = recent.Where(r => r.CreatedByUserId.HasValue).Select(r => r.CreatedByUserId.Value).Distinct().ToList();

        var customers = new List<(int CustomerId, string FullName)>();
        if (customerIds.Any())
        {
            customers = await _context.Customers
                .Where(c => customerIds.Contains(c.CustomerId))
                .Select(c => new { c.CustomerId, c.FullName })
                .AsNoTracking()
                .ToListAsync()
                .ContinueWith(t => t.Result.Select(x => (x.CustomerId, x.FullName)).ToList());
        }

        var users = new List<(int UserId, string FullName, string Username)>();
        if (userIds.Any())
        {
            users = await _context.Users
                .Where(u => userIds.Contains(u.UserId))
                .Select(u => new { u.UserId, u.FullName, u.Username })
                .AsNoTracking()
                .ToListAsync()
                .ContinueWith(t => t.Result.Select(x => (x.UserId, x.FullName, x.Username)).ToList());
        }

        foreach (var r in recent)
        {
            var dto = new RecentOrderDto
            {
                OrderCode = $"#ORD-{r.OrderId:00000}",
                OrderDate = r.OrderDate,
                Status = r.Status,
                Total = r.Total
            };

            if (r.CustomerId.HasValue)
            {
                var cust = customers.FirstOrDefault(c => c.CustomerId == r.CustomerId.Value);
                if (!string.IsNullOrEmpty(cust.FullName))
                    dto.StaffOrCustomer = cust.FullName;
            }

            if (string.IsNullOrEmpty(dto.StaffOrCustomer) && r.CreatedByUserId.HasValue)
            {
                var usr = users.FirstOrDefault(u => u.UserId == r.CreatedByUserId.Value);
                if (!string.IsNullOrEmpty(usr.FullName))
                    dto.StaffOrCustomer = usr.FullName;
                else if (!string.IsNullOrEmpty(usr.Username))
                    dto.StaffOrCustomer = usr.Username;
            }

            if (string.IsNullOrEmpty(dto.StaffOrCustomer))
                dto.StaffOrCustomer = "Khách lẻ";

            model.RecentOrders.Add(dto);
        }

        // Revenue by last 7 days
        var days = Enumerable.Range(0, 7).Select(i => today.AddDays(-6 + i)).ToList();
        var start = days.First();
        var end = days.Last().AddDays(1);

        var sums = await _context.Orders
            .Where(o => o.OrderDate >= start && o.OrderDate < end)
            .GroupBy(o => o.OrderDate.Date)
            .Select(g => new { Day = g.Key, Sum = g.Sum(x => (decimal?)x.Total) ?? 0m })
            .ToListAsync();

        foreach (var d in days)
        {
            var rec = sums.FirstOrDefault(s => s.Day == d);
            model.RevenueByDayLabels.Add(d.ToString("MM-dd"));
            model.RevenueByDay.Add(rec?.Sum ?? 0m);
        }

        // (TÙY CHỌN) Build StockLabels/StockData nếu bạn muốn donut chart.
        // model.StockLabels = new List<string>{ "Còn hàng","Sắp hết","Hết" };
        // model.StockData = new List<int>{ x1,x2,x3 };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        // Bảo vệ: không tắt admin chính (nếu cần)
        user.IsActive = !user.IsActive;
        _context.Users.Update(user);
        await _context.SaveChangesAsync();

        return RedirectToAction("StaffList");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return NotFound();

        // Bảo vệ: không xóa tài khoản admin hệ thống (giả sử RoleId == 1)
        if (user.RoleId == 1)
        {
            TempData["Error"] = "Không thể xóa tài khoản admin hệ thống.";
            return RedirectToAction("StaffList");
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return RedirectToAction("StaffList");
    }

    public async Task<IActionResult> StaffList()
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .Where(u => u.RoleId != 3) // nếu bạn muốn loại trừ role Customer (role id = 3)
            .AsNoTracking()
            .ToListAsync();

        // TRÁNH trả tên partial không đúng: tên file trong project của bạn là "StaffListPartial.cshtml"
        return PartialView("StaffListPartial", users);
    }
}
