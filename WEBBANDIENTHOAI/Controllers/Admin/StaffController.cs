using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.ViewModels;
using WEBBANDIENTHOAI.Helpers;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using WEBBANDIENTHOAI.Repository.TaiKhoan;

namespace WEBBANDIENTHOAI.Controllers.Admin
{
    public class StaffController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IUserRepository _userRepository;

        public StaffController(AppDbContext context, IUserRepository userRepository)
        {
            _context = context;
            _userRepository = userRepository;
        }

        // GET: Staff/StaffList
        public async Task<IActionResult> StaffList()
        {
            var users = await _userRepository.GetStaffUsersAsync();
            ViewBag.Roles = await _context.Roles.Where(r => r.RoleId != 3).ToListAsync();
            return View("~/Views/Admin/StaffListPartial.cshtml", users);
        }

        // GET: Lấy thông tin user (Dùng Query String nên không cần [FromForm])
        [HttpGet]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user == null) return Json(new { success = false, message = "Không tìm thấy nhân viên" });

            return Json(new
            {
                success = true,
                data = new
                {
                    user.UserId,
                    user.Username,
                    user.FullName,
                    user.Email,
                    user.RoleId,
                    user.IsActive
                }
            });
        }

        // POST: Tạo nhân viên mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser([FromForm] UserViewModel model) //
        {
            if (ModelState.IsValid)
            {
                if (await _userRepository.UsernameExistsAsync(model.Username))
                    return Json(new { success = false, message = "Tên đăng nhập đã tồn tại" });

                if (await _userRepository.EmailExistsAsync(model.Email))
                    return Json(new { success = false, message = "Email đã tồn tại" });

                var user = new User
                {
                    Username = model.Username,
                    PasswordHash = SecurityHelper.HashPassword(model.Password),
                    FullName = model.FullName,
                    Email = model.Email,
                    RoleId = model.RoleId,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow
                };

                if (await _userRepository.CreateUserAsync(user))
                    return Json(new { success = true, message = "Thêm nhân viên thành công!" });

                return Json(new { success = false, message = "Lỗi Database: Không thể lưu nhân viên (Kiểm tra RoleId hoặc độ dài chuỗi)." });
            }

            var errors = string.Join("<br/>", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return Json(new { success = false, message = errors });
        }

        // POST: Cập nhật nhân viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, [FromForm] UserViewModel model) //
        {
            if (id != model.UserId) return Json(new { success = false, message = "ID không khớp" });

            // Fix: Bỏ qua lỗi password nếu để trống khi edit
            if (string.IsNullOrEmpty(model.Password)) ModelState.Remove("Password");

            if (ModelState.IsValid)
            {
                if (await _userRepository.UsernameExistsAsync(model.Username, id))
                    return Json(new { success = false, message = "Tên đăng nhập đã tồn tại" });

                if (await _userRepository.EmailExistsAsync(model.Email, id))
                    return Json(new { success = false, message = "Email đã tồn tại" });

                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null) return Json(new { success = false, message = "Không tìm thấy nhân viên" });

                user.Username = model.Username;
                user.FullName = model.FullName;
                user.Email = model.Email;
                user.RoleId = model.RoleId;
                user.IsActive = model.IsActive;

                if (!string.IsNullOrEmpty(model.Password))
                {
                    user.PasswordHash = SecurityHelper.HashPassword(model.Password);
                }

                if (await _userRepository.UpdateUserAsync(user))
                    return Json(new { success = true, message = "Cập nhật thành công!" });

                return Json(new { success = false, message = "Lỗi Database khi cập nhật." });
            }

            var errors = string.Join("<br/>", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return Json(new { success = false, message = errors });
        }

        // POST: Xóa nhân viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser([FromForm] int userId) // Thêm [FromForm] là BẮT BUỘC để đọc từ FormData
        {
            // Kiểm tra user có tồn tại không trước khi xóa
            if (!await _userRepository.UserExistsAsync(userId))
            {
                return Json(new { success = false, message = "Nhân viên không tồn tại hoặc đã bị xóa." });
            }

            var result = await _userRepository.DeleteUserAsync(userId);
            if (result)
            {
                return Json(new { success = true, message = "Xóa nhân viên thành công!" });
            }
            return Json(new { success = false, message = "Không thể xóa: Có thể nhân viên này là Admin hoặc đã có đơn hàng/dữ liệu liên quan." });
        }

        // POST: Toggle Active
        [HttpPost]
        public async Task<IActionResult> ToggleActive([FromForm] int userId) //
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null) return Json(new { success = false, message = "Không tìm thấy nhân viên" });

            if (user.RoleId == 1) return Json(new { success = false, message = "Không thể tắt tài khoản Admin hệ thống" });

            user.IsActive = !user.IsActive;
            if (await _userRepository.UpdateUserAsync(user))
            {
                return Json(new { success = true, message = "Đổi trạng thái thành công!", isActive = user.IsActive });
            }
            return Json(new { success = false, message = "Lỗi Database." });
        }
    }
}