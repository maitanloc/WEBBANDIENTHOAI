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

        // ĐÃ SỬA – dùng StatusId thay vì Status string
        model.NewOrdersCount = await _context.Orders
            .CountAsync(o => o.StatusId == 1);   // 1 = Pending (theo dữ liệu bạn đã insert)

        // Low stock: inventory current <= minimum
        // LƯU Ý: nếu DbSet tên khác (Inventories) -> đổi _context.Inventory thành _context.Inventories
        // Ở đây mình giữ _context.Inventory theo code bạn đang dùng.
        model.LowStockCount = await _context.Inventory
            .Where(i => i.CurrentQuantity <= i.MinimumQuantity)
            .CountAsync();

        // Recent orders (top 6) – ĐÃ SỬA HOÀN CHỈNH CHO StatusId + OrderStatus
        var recentOrders = await _context.Orders
            .Include(o => o.Customer)           // Lấy tên khách hàng
            .Include(o => o.OrderStatus)        // Lấy tên trạng thái (Pending, Shipped,...)
            .OrderByDescending(o => o.OrderDate)
            .Take(6)
            .Select(o => new
            {
                o.OrderId,
                o.OrderDate,
                o.Total,
                StatusName = o.OrderStatus != null ? o.OrderStatus.StatusName : "Không xác định",
                CustomerName = o.Customer != null ? o.Customer.FullName : null,
                o.CreatedByUserId
            })
            .ToListAsync();

        // Lấy tên nhân viên nếu đơn do nhân viên tạo (CreatedByUserId)
        var staffIds = recentOrders
            .Where(x => x.CreatedByUserId.HasValue)
            .Select(x => x.CreatedByUserId.Value)
            .Distinct()
            .ToList();

        var staffDict = new Dictionary<int, string>();
        if (staffIds.Any())
        {
            staffDict = await _context.Users
                .Where(u => staffIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => u.FullName ?? u.Username ?? "Nhân viên");
        }

        // Đổ dữ liệu vào DTO
        foreach (var r in recentOrders)
        {
            var dto = new RecentOrderDto
            {
                OrderCode = $"#ORD-{r.OrderId:00000}",
                OrderDate = r.OrderDate,
                Status = r.StatusName,                                           // ĐÃ SỬA: hiển thị tên trạng thái
                Total = r.Total,   // XÓA HẾT ?? 0m đi, chỉ để thế này là xong! // XÓA HẾT ?? 0m đi, chỉ để thế này là xong!
                StaffOrCustomer = r.CustomerName
                    ?? (r.CreatedByUserId.HasValue && staffDict.ContainsKey(r.CreatedByUserId.Value)
                        ? staffDict[r.CreatedByUserId.Value]
                        : "Khách lẻ")
            };

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

    // GET: Admin/StaffList - Quản lý nhân viên
    public async Task<IActionResult> StaffList()
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .Where(u => u.RoleId != 3) // Loại trừ Customer
            .ToListAsync();

        ViewBag.Roles = await _context.Roles.Where(r => r.RoleId != 3).ToListAsync();
        return View("StaffListPartial", users);
    }
}
