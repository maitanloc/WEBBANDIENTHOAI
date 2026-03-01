// Controllers/SpinController.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Controllers
{
    public class SpinController : Controller
    {
        private readonly AppDbContext _ctx;
        private static readonly Random _rng = new();

        // Ô "Chúc may mắn lần sau" (không có voucher)
        private const string NO_PRIZE_LABEL = "😢 Chúc may mắn!";

        // Tỉ lệ không trúng (số ô không giải / tổng ô)
        private const int NO_PRIZE_SLOTS = 2;

        public SpinController(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        // ─── GET /Spin → redirect to History (admin) ───────────────────────────
        public IActionResult Index() => RedirectToAction("History");

        // ─── Shared helper: lấy danh sách voucher theo cùng 1 query ────────────────
        private async Task<List<Voucher>> GetActiveVouchersAsync(DateTime now) =>
            await _ctx
                .Vouchers.Where(v => v.IsActive && v.EndDate >= now)
                .OrderBy(v => v.VoucherId) // thứ tự cố định, server = client
                .ToListAsync();

        // ─── POST /Spin/DoSpin ─────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoSpin()
        {
            var cidStr = HttpContext.Session.GetString("CustomerId");
            int? customerId = int.TryParse(cidStr, out var cid) ? cid : (int?)null;
            if (!customerId.HasValue)
                return Json(new { success = false, message = "Bạn cần đăng nhập để quay." });

            // Kiểm tra đã quay hôm nay chưa
            var now = DateTime.UtcNow;
            var startOfDay = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
            bool spunToday = await _ctx.SpinHistories.AnyAsync(s =>
                s.CustomerId == customerId && s.SpinAt >= startOfDay
            );

            if (spunToday)
                return Json(
                    new
                    {
                        success = false,
                        message = "Bạn đã dùng lượt quay hôm nay rồi. Quay lại vào ngày mai nhé! 🌙",
                    }
                );

            // ── Dùng CÙNG query với GetSegments ──────────────────────────────────
            var vouchers = await GetActiveVouchersAsync(now);
            int totalSlots = vouchers.Count + NO_PRIZE_SLOTS;

            // Tạo danh sách nhãn để client đồng bộ (server là nguồn sự thật)
            var segmentLabels = vouchers
                .Select(v =>
                    v.DiscountType == DiscountType.Percent ? $"🎉 {v.Value}%" : $"🎉 {v.Value:N0}đ"
                )
                .Concat(Enumerable.Repeat("😢 Thử lại!", NO_PRIZE_SLOTS))
                .ToList();

            // Random chọn ô
            int winIndex = _rng.Next(totalSlots);
            bool isWin = winIndex < vouchers.Count;

            if (isWin)
            {
                var wonVoucher = vouchers[winIndex];
                string label = segmentLabels[winIndex];

                // Gán voucher vào kho user
                bool alreadyHas = await _ctx.UserVouchers.AnyAsync(uv =>
                    uv.CustomerId == customerId
                    && uv.VoucherId == wonVoucher.VoucherId
                    && !uv.IsUsed
                );

                if (!alreadyHas)
                {
                    _ctx.UserVouchers.Add(
                        new UserVoucher
                        {
                            CustomerId = customerId.Value,
                            VoucherId = wonVoucher.VoucherId,
                            AssignedAt = now,
                            IsUsed = false,
                        }
                    );
                }

                _ctx.SpinHistories.Add(
                    new SpinHistory
                    {
                        CustomerId = customerId.Value,
                        VoucherId = wonVoucher.VoucherId,
                        SpinAt = now,
                        SpinResult = label,
                    }
                );
                await _ctx.SaveChangesAsync();

                return Json(
                    new
                    {
                        success = true,
                        isWin = true,
                        segmentIndex = winIndex,
                        segmentLabels, // Client dùng list này → không bao giờ lệch
                        label,
                        voucherCode = wonVoucher.Code,
                        message = $"🎊 Chúc mừng! Bạn đã nhận được mã giảm giá <strong>{wonVoucher.Code}</strong>!",
                    }
                );
            }
            else
            {
                string label = NO_PRIZE_LABEL;
                _ctx.SpinHistories.Add(
                    new SpinHistory
                    {
                        CustomerId = customerId.Value,
                        VoucherId = null,
                        SpinAt = now,
                        SpinResult = label,
                    }
                );
                await _ctx.SaveChangesAsync();

                return Json(
                    new
                    {
                        success = true,
                        isWin = false,
                        segmentIndex = winIndex,
                        segmentLabels,
                        label,
                        message = "Chúc may mắn lần sau! Hãy quay lại vào ngày mai nhé 🌙",
                    }
                );
            }
        }

        // ─── GET /Spin/History (Admin) ─────────────────────────────────────────
        public async Task<IActionResult> History(int page = 1, string? search = null)
        {
            const int pageSize = 20;

            var query = _ctx
                .SpinHistories.Include(s => s.Customer)
                .Include(s => s.Voucher)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(s =>
                    s.Customer != null
                    && (s.Customer.FullName.Contains(search) || s.Customer.Email.Contains(search))
                );

            int total = await query.CountAsync();

            var histories = await query
                .OrderByDescending(s => s.SpinAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;
            ViewBag.Search = search;
            ViewBag.TotalPage = (int)Math.Ceiling(total / (double)pageSize);

            return View(histories);
        }

        // ─── GET /Spin/GetSegments ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetSegments()
        {
            var now = DateTime.UtcNow;
            var vouchers = await _ctx
                .Vouchers.Where(v => v.IsActive && v.EndDate >= now)
                .Select(v => new
                {
                    v.VoucherId,
                    v.Code,
                    v.Description,
                    v.DiscountType,
                    v.Value,
                })
                .ToListAsync();
            return Json(vouchers);
        }
    }
}
