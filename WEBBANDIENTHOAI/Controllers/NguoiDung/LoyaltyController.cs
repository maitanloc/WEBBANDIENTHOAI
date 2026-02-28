// WEBBANDIENTHOAI/Controllers/NguoiDung/LoyaltyController.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Services;

namespace WEBBANDIENTHOAI.Controllers.NguoiDung
{
    public class LoyaltyController : Controller
    {
        private readonly AppDbContext      _ctx;
        private readonly IPromotionService _svc;

        public LoyaltyController(AppDbContext ctx, IPromotionService svc)
        {
            _ctx = ctx;
            _svc = svc;
        }

        private int? GetCustomerId()
        {
            var role = HttpContext.Session.GetString("RoleName");
            var idStr = HttpContext.Session.GetString("UserId");
            if (role != "Customer" || string.IsNullOrEmpty(idStr)) return null;
            return int.TryParse(idStr, out int id) ? id : (int?)null;
        }

        private void SetLayoutViewBag(Customer c)
        {
            ViewBag.IsLoggedIn     = true;
            ViewBag.CustomerName   = c.FullName;
            ViewBag.CustomerEmail  = c.Email;
            ViewBag.CustomerPhone  = c.Phone;
        }

        // ────────────────────────────────────────────────────────────────
        // GET /Loyalty/Index  – Dashboard điểm thưởng
        // ────────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            var customerId = GetCustomerId();
            if (customerId == null) return RedirectToAction("Login", "Account");

            var customer = await _ctx.Customers
                .Include(c => c.Tier)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);

            if (customer == null) return RedirectToAction("Login", "Account");
            SetLayoutViewBag(customer);

            // Lịch sử điểm (mới nhất trước)
            var history = await _ctx.UserPointHistories
                .Where(h => h.CustomerId == customerId)
                .OrderByDescending(h => h.CreatedAt)
                .Take(30)
                .ToListAsync();

            // Tier tiếp theo (dựa trên TotalSpent)
            var tiers = await _ctx.CustomerTiers.OrderBy(t => t.MinSpending).ToListAsync();
            var nextTier = tiers.FirstOrDefault(t => t.MinSpending > customer.TotalSpent);

            ViewBag.Customer  = customer;
            ViewBag.History   = history;
            ViewBag.Tiers     = tiers;
            ViewBag.NextTier  = nextTier;

            return View("~/Views/Loyalty/Index.cshtml");
        }

        // ────────────────────────────────────────────────────────────────
        // GET /Loyalty/MyVouchers  – Kho voucher của user
        // ────────────────────────────────────────────────────────────────
        public async Task<IActionResult> MyVouchers()
        {
            var customerId = GetCustomerId();
            if (customerId == null) return RedirectToAction("Login", "Account");

            var customer = await _ctx.Customers.FindAsync(customerId);
            if (customer == null) return RedirectToAction("Login", "Account");
            SetLayoutViewBag(customer);

            var now = DateTime.UtcNow;

            var userVouchers = await _ctx.UserVouchers
                .Include(uv => uv.Voucher)
                .Where(uv => uv.CustomerId == customerId && !uv.IsUsed
                          && uv.Voucher.IsActive
                          && uv.Voucher.EndDate >= now)
                .OrderByDescending(uv => uv.AssignedAt)
                .ToListAsync();

            ViewBag.Customer = customer;
            return View("~/Views/Loyalty/MyVouchers.cshtml", userVouchers);
        }

        // ────────────────────────────────────────────────────────────────
        // POST /Loyalty/ApplyVoucher  (AJAX – từ cart / checkout)
        // ────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyVoucher(string code, decimal orderTotal)
        {
            var customerId = GetCustomerId();
            if (customerId == null)
                return Json(new { success = false, message = "Vui lòng đăng nhập." });

            var result = await _svc.ValidateVoucherAsync(code, orderTotal, customerId.Value);

            if (!result.IsValid)
                return Json(new { success = false, message = result.Message });

            // Lưu vào session để dùng tại checkout
            HttpContext.Session.SetString("AppliedVoucherCode", result.Voucher!.Code);
            HttpContext.Session.SetInt32("AppliedVoucherId",    result.Voucher.VoucherId);
            HttpContext.Session.SetString("AppliedVoucherDiscount", result.DiscountAmount.ToString());

            return Json(new
            {
                success        = true,
                message        = result.Message,
                discountAmount = result.DiscountAmount,
                voucherId      = result.Voucher.VoucherId,
                code           = result.Voucher.Code
            });
        }

        // ────────────────────────────────────────────────────────────────
        // POST /Loyalty/RemoveVoucher  (AJAX)
        // ────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveVoucher()
        {
            HttpContext.Session.Remove("AppliedVoucherCode");
            HttpContext.Session.Remove("AppliedVoucherId");
            HttpContext.Session.Remove("AppliedVoucherDiscount");
            return Json(new { success = true });
        }

        // ────────────────────────────────────────────────────────────────
        // POST /Loyalty/ApplyPoints  (AJAX – dùng điểm tại checkout)
        // ────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyPoints(int pointsToUse)
        {
            var customerId = GetCustomerId();
            if (customerId == null)
                return Json(new { success = false, message = "Vui lòng đăng nhập." });

            if (pointsToUse <= 0)
                return Json(new { success = false, message = "Số điểm phải lớn hơn 0." });

            var customer = await _ctx.Customers.FindAsync(customerId);
            if (customer == null)
                return Json(new { success = false, message = "Không tìm thấy tài khoản." });

            int actualPoints = Math.Min(pointsToUse, customer.LoyaltyPoints);
            decimal moneyValue = actualPoints * 1000m;

            HttpContext.Session.SetInt32("AppliedPoints",       actualPoints);
            HttpContext.Session.SetString("AppliedPointsMoney", moneyValue.ToString());

            return Json(new
            {
                success       = true,
                pointsUsed    = actualPoints,
                moneyValue    = moneyValue,
                remainPoints  = customer.LoyaltyPoints - actualPoints,
                message       = $"Áp dụng {actualPoints:N0} điểm = {moneyValue:N0}đ giảm."
            });
        }

        // ────────────────────────────────────────────────────────────────
        // POST /Loyalty/RemovePoints (AJAX)
        // ────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemovePoints()
        {
            HttpContext.Session.Remove("AppliedPoints");
            HttpContext.Session.Remove("AppliedPointsMoney");
            return Json(new { success = true });
        }
    }
}
