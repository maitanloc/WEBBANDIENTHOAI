using Microsoft.AspNetCore.Mvc;
using WEBBANDIENTHOAI.Data;
using Microsoft.AspNetCore.Http;
using System.Linq;
using WEBBANDIENTHOAI.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.ViewModels;
using WEBBANDIENTHOAI.Services;

namespace WEBBANDIENTHOAI.Controllers
{
    // Adding explicit routing
    [Route("Info")]
    public class InfoController : Controller
    {
        private readonly AppDbContext _context;

        public InfoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("ThongTinKH")]
        public IActionResult ThongTinKH()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var role = HttpContext.Session.GetString("RoleName");
            if (role != "Customer")
            {
                return RedirectToAction("Index", "Home");
            }

            if (!int.TryParse(userId, out int customerId))
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = _context.Customers.AsNoTracking().FirstOrDefault(c => c.CustomerId == customerId);

            if (customer == null)
            {
                return NotFound();
            }

            // Calculate total spending for orders with StatusId = 4
            var totalSpending = _context.Orders
                                        .Where(o => o.CustomerId == customerId && o.StatusId == 4)
                                        .Sum(o => (decimal?)o.Total) ?? 0m;

            ViewBag.TotalSpending = totalSpending;

            // Populate ViewBag for the layout
            ViewBag.CustomerName = customer.FullName;
            ViewBag.CustomerEmail = customer.Email;
            ViewBag.CustomerPhone = customer.Phone;
            ViewBag.IsLoggedIn = true;

            return View(customer);
        }

        [HttpPost("UpdateProfile")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(Customer model)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(userId) || role != "Customer" || model.CustomerId.ToString() != userId)
            {
                return StatusCode(401, new { success = false, message = "Unauthorized access." });
            }
            
            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                 return BadRequest(new { success = false, message = "Họ và Tên là bắt buộc." });
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
            
            try
            {
                await _context.SaveChangesAsync();
                var currentUsername = HttpContext.Session.GetString("Username");
                if (currentUsername != customerToUpdate.FullName)
                {
                    HttpContext.Session.SetString("Username", customerToUpdate.FullName);
                }
                
                return Ok(new { success = true, newName = customerToUpdate.FullName });
            }
            catch (DbUpdateException ex)
            {
                // In a real app, you would log this exception.
                return StatusCode(500, new { success = false, message = "Lỗi khi lưu vào cơ sở dữ liệu." });
            }
        }

        [HttpPost("ChangePassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(new { success = false, message = string.Join(" ", errors) });
            }

            var userId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(userId) || role != "Customer" || !int.TryParse(userId, out int customerId))
            {
                return StatusCode(401, new { success = false, message = "Unauthorized access." });
            }

            var customer = await _context.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return NotFound(new { success = false, message = "Không tìm thấy khách hàng." });
            }

            // Verify current password
            if (!PasswordHasher.Verify(model.CurrentPassword, customer.PasswordHash))
            {
                return BadRequest(new { success = false, message = "Mật khẩu hiện tại không đúng." });
            }

            // Hash and update new password
            customer.PasswordHash = PasswordHasher.Hash(model.NewPassword);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Đổi mật khẩu thành công." });
            }
            catch (DbUpdateException)
            {
                return StatusCode(500, new { success = false, message = "Lỗi khi cập nhật mật khẩu." });
            }
        }
    }
}
