using Microsoft.AspNetCore.Mvc;
using WEBBANDIENTHOAI.Data;
using Microsoft.AspNetCore.Http;
using System.Linq;
using WEBBANDIENTHOAI.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.ViewModels;
using WEBBANDIENTHOAI.Services;
using System.Collections.Generic;

namespace WEBBANDIENTHOAI.Controllers
{
    [Route("Info")]
    public class InfoController : Controller
    {
        private readonly AppDbContext _context;

        public InfoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("ThongTinKH")]
        public async Task<IActionResult> ThongTinKH()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out int customerId))
            {
                return RedirectToAction("LoginRegister", "Account");
            }

            var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customer == null)
            {
                return NotFound();
            }

            var viewModel = new ProfileViewModel
            {
                CustomerId = customer.CustomerId,
                FullName = customer.FullName,
                Email = customer.Email,
                Phone = customer.Phone,
                CitizenID = customer.CitizenID,
                Address = customer.Address,
                CreatedAt = customer.CreatedAt,
                IsActive = customer.IsActive
            };

            await PopulateViewBagForCustomer(customerId);
            return View(viewModel);
        }

        [HttpPost("UpdateProfile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId) || model.CustomerId.ToString() != userId)
            {
                return Unauthorized(new { success = false, message = "Unauthorized access." });
            }

            ModelState.Remove(nameof(model.PasswordHash)); // Ignore password hash validation
            ModelState.Remove(nameof(model.CurrentPassword));
            ModelState.Remove(nameof(model.NewPassword));
            ModelState.Remove(nameof(model.ConfirmNewPassword));

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = GetModelStateErrors() });
            }

            var customerToUpdate = await _context.Customers.FindAsync(model.CustomerId);
            if (customerToUpdate == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy khách hàng." });
            }

            customerToUpdate.FullName = model.FullName;
            customerToUpdate.Phone = model.Phone;
            customerToUpdate.Address = model.Address;
            customerToUpdate.CitizenID = model.CitizenID;

            await _context.SaveChangesAsync();

            HttpContext.Session.SetString("Username", customerToUpdate.FullName);
            return Ok(new { success = true, newName = customerToUpdate.FullName, message = "Cập nhật thông tin thành công!" });
        }

        [HttpPost("ChangePassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ProfileViewModel model)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId) || !int.TryParse(userId, out var customerId) || model.CustomerId != customerId)
            {
                return Unauthorized(new { success = false, message = "Unauthorized access." });
            }

            // For this action, only validate password-related fields.
            // Remove other required fields from the base Customer model from validation.
            ModelState.Remove(nameof(model.FullName));
            ModelState.Remove(nameof(model.Email));
            ModelState.Remove(nameof(model.Phone));
            ModelState.Remove(nameof(model.CitizenID));
            ModelState.Remove(nameof(model.Address));
            ModelState.Remove(nameof(model.PasswordHash));
            
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, errors = GetModelStateErrors() });
            }

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy khách hàng." });
            }

            if (!PasswordHasher.Verify(model.CurrentPassword, customer.PasswordHash))
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Mật khẩu hiện tại không đúng.");
                return Json(new { success = false, errors = GetModelStateErrors() });
            }

            customer.PasswordHash = PasswordHasher.Hash(model.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Đổi mật khẩu thành công." });
        }

        private Dictionary<string, string> GetModelStateErrors()
        {
            return ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).FirstOrDefault()
            );
        }

        private async Task PopulateViewBagForCustomer(int customerId)
        {
            var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.CustomerId == customerId);
            if (customer != null)
            {
                var totalSpending = await _context.Orders
                                            .Where(o => o.CustomerId == customerId && o.StatusId == 4)
                                            .SumAsync(o => (decimal?)o.Total) ?? 0m;

                ViewBag.TotalSpending = totalSpending;
                ViewBag.CustomerName = customer.FullName;
                ViewBag.CustomerEmail = customer.Email;
                ViewBag.CustomerPhone = customer.Phone;
                ViewBag.IsLoggedIn = true;
            }
        }
    }
}
