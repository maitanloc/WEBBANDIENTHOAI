using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Models;

namespace WebBanDienThoai.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // Helper: hash plain password to SHA-256 bytes
        private static byte[] HashPassword(string plain)
        {
            if (plain == null) plain = string.Empty;
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(plain));
        }

        public IActionResult Index()
        {
            // only admin allowed
            if (HttpContext.Session.GetString("RoleName") != "Admin")
                return RedirectToAction("Login", "Account");

            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.TotalCustomers = _context.Customers.Count();
            ViewBag.TotalProducts = _context.Products.Count();

            // load all users with role info (optionally filter out super-admin if needed)
            var staff = _context.Users.Include(u => u.Role).ToList();
            return View(staff);
        }

        [HttpGet]
        public IActionResult AddStaff()
        {
            if (HttpContext.Session.GetString("RoleName") != "Admin")
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddStaff(string username, string password, string fullname, string email)
        {
            // basic validation
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Username và mật khẩu là bắt buộc.";
                return View();
            }

            // check username/email unique
            if (_context.Users.Any(u => u.Username == username))
            {
                ViewBag.Error = "Username đã tồn tại!";
                return View();
            }
            if (!string.IsNullOrWhiteSpace(email) && _context.Users.Any(u => u.Email == email))
            {
                ViewBag.Error = "Email đã tồn tại!";
                return View();
            }

            var staffRole = _context.Roles.FirstOrDefault(r => r.RoleName == "Staff");
            if (staffRole == null)
            {
                ViewBag.Error = "Không tìm thấy quyền 'Staff'. Vui lòng tạo Role trước.";
                return View();
            }

            var user = new User
            {
                Username = username,
                PasswordHash = HashPassword(password), // store hashed password
                FullName = string.IsNullOrWhiteSpace(fullname) ? null : fullname,
                Email = string.IsNullOrWhiteSpace(email) ? null : email,
                RoleId = staffRole.RoleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                _context.Users.Add(user);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                // log ex if you have logger
                ViewBag.Error = "Lỗi khi thêm nhân viên: " + ex.Message;
                return View();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ToggleActive(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user != null)
            {
                user.IsActive = !user.IsActive;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteUser(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
