// WEBBANDIENTHOAI/Controllers/Admin/VouchersAdminController.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Controllers.Admin
{
    [Area("Admin")]
    public class VouchersAdminController : Controller
    {
        private readonly AppDbContext _ctx;

        public VouchersAdminController(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("RoleName");
            return role == "Admin" || role == "Staff";
        }

        // ────────────────────────────────────────────────────────────────
        // GET /VouchersAdmin
        // ────────────────────────────────────────────────────────────────
        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var vouchers = await _ctx.Vouchers
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            return View("~/Views/Admin/VoucherAdmin/Index.cshtml", vouchers);
        }

        // ────────────────────────────────────────────────────────────────
        // GET /VouchersAdmin/Create
        // ────────────────────────────────────────────────────────────────
        public IActionResult Create()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            return View("~/Views/Admin/VoucherAdmin/Create.cshtml", new Voucher
            {
                StartDate = DateTime.Today,
                EndDate   = DateTime.Today.AddMonths(3)
            });
        }

        // ────────────────────────────────────────────────────────────────
        // POST /VouchersAdmin/Create
        // ────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Voucher model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            // Chuẩn hoá code
            model.Code = model.Code.Trim().ToUpper();

            // Kiểm tra code trùng
            bool codeExist = await _ctx.Vouchers.AnyAsync(v => v.Code == model.Code);
            if (codeExist)
                ModelState.AddModelError("Code", "Mã voucher này đã tồn tại trong hệ thống.");

            // Percent: MaxDiscountAmount bắt buộc
            if (model.DiscountType == DiscountType.Percent && (!model.MaxDiscountAmount.HasValue || model.MaxDiscountAmount <= 0))
                ModelState.AddModelError("MaxDiscountAmount", "Vui lòng nhập số tiền giảm tối đa cho loại Percent.");

            if (!ModelState.IsValid)
                return View("~/Views/Admin/VoucherAdmin/Create.cshtml", model);

            model.UsedCount = 0;
            model.IsActive  = true;
            model.CreatedAt = DateTime.UtcNow;
            model.StartDate = DateTime.SpecifyKind(model.StartDate, DateTimeKind.Utc);
            model.EndDate   = DateTime.SpecifyKind(model.EndDate, DateTimeKind.Utc);

            _ctx.Vouchers.Add(model);
            await _ctx.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã tạo voucher [{model.Code}] thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ────────────────────────────────────────────────────────────────
        // GET /VouchersAdmin/Edit/{id}
        // ────────────────────────────────────────────────────────────────
        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var voucher = await _ctx.Vouchers.FindAsync(id);
            if (voucher == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy voucher.";
                return RedirectToAction(nameof(Index));
            }

            return View("~/Views/Admin/VoucherAdmin/Edit.cshtml", voucher);
        }

        // ────────────────────────────────────────────────────────────────
        // POST /VouchersAdmin/Edit/{id}
        // ────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Voucher model)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");
            if (id != model.VoucherId) return BadRequest();

            model.Code = model.Code.Trim().ToUpper();

            bool codeExist = await _ctx.Vouchers
                .AnyAsync(v => v.Code == model.Code && v.VoucherId != id);
            if (codeExist)
                ModelState.AddModelError("Code", "Mã voucher này đã được sử dụng bởi voucher khác.");

            if (model.DiscountType == DiscountType.Percent && (!model.MaxDiscountAmount.HasValue || model.MaxDiscountAmount <= 0))
                ModelState.AddModelError("MaxDiscountAmount", "Vui lòng nhập số tiền giảm tối đa.");

            if (!ModelState.IsValid)
                return View("~/Views/Admin/VoucherAdmin/Edit.cshtml", model);

            var existing = await _ctx.Vouchers.FindAsync(id);
            if (existing == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy voucher.";
                return RedirectToAction(nameof(Index));
            }

            existing.Code             = model.Code;
            existing.Description      = model.Description;
            existing.DiscountType     = model.DiscountType;
            existing.Value            = model.Value;
            existing.MaxDiscountAmount = model.MaxDiscountAmount;
            existing.MinOrderValue    = model.MinOrderValue;
            existing.StartDate        = DateTime.SpecifyKind(model.StartDate, DateTimeKind.Utc);
            existing.EndDate          = DateTime.SpecifyKind(model.EndDate, DateTimeKind.Utc);
            existing.Quantity         = model.Quantity;
            existing.IsActive         = model.IsActive;

            await _ctx.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã cập nhật voucher [{existing.Code}].";
            return RedirectToAction(nameof(Index));
        }

        // ────────────────────────────────────────────────────────────────
        // POST /VouchersAdmin/Delete/{id}
        // ────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var voucher = await _ctx.Vouchers.FindAsync(id);
            if (voucher == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy voucher.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra đã được dùng chưa
            if (voucher.UsedCount > 0)
            {
                TempData["ErrorMessage"] = "Không thể xoá voucher đã được sử dụng. Hãy vô hiệu hoá thay vì xoá.";
                return RedirectToAction(nameof(Index));
            }

            _ctx.Vouchers.Remove(voucher);
            await _ctx.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã xoá voucher [{voucher.Code}].";
            return RedirectToAction(nameof(Index));
        }

        // ────────────────────────────────────────────────────────────────
        // POST /VouchersAdmin/Toggle/{id}   – kích hoạt / vô hiệu hóa
        // ────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Account");

            var voucher = await _ctx.Vouchers.FindAsync(id);
            if (voucher == null)
                return Json(new { success = false, message = "Không tìm thấy voucher." });

            voucher.IsActive = !voucher.IsActive;
            await _ctx.SaveChangesAsync();

            return Json(new { success = true, isActive = voucher.IsActive,
                message = voucher.IsActive ? "Voucher đã được kích hoạt." : "Voucher đã bị vô hiệu hoá." });
        }

        // ────────────────────────────────────────────────────────────────
        // GET /VouchersAdmin/AssignToUser – giao voucher cho khách hàng
        // ────────────────────────────────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignToUser(int voucherId, int customerId)
        {
            if (!IsAdmin()) return Json(new { success = false, message = "Không có quyền." });

            var voucher  = await _ctx.Vouchers.FindAsync(voucherId);
            var customer = await _ctx.Customers.FindAsync(customerId);
            if (voucher == null || customer == null)
                return Json(new { success = false, message = "Không tìm thấy voucher hoặc khách hàng." });

            bool already = await _ctx.UserVouchers
                .AnyAsync(uv => uv.CustomerId == customerId && uv.VoucherId == voucherId && !uv.IsUsed);
            if (already)
                return Json(new { success = false, message = "Khách hàng đã có voucher này trong kho." });

            _ctx.UserVouchers.Add(new UserVoucher
            {
                CustomerId = customerId,
                VoucherId  = voucherId,
                AssignedAt = DateTime.UtcNow,
                IsUsed     = false
            });
            await _ctx.SaveChangesAsync();
            return Json(new { success = true, message = $"Đã giao voucher {voucher.Code} cho KH #{customerId}." });
        }
    }
}
