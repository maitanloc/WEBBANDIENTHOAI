using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Services;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // nếu đã login -> redirect
            if (HttpContext.Session.GetString("UserId") != null)
            {
                var role = HttpContext.Session.GetString("RoleName") ?? "";
                if (role == "Admin") return RedirectToAction("Index", "Admin");
                if (role == "Staff") return RedirectToAction("Index", "Staff");
                return RedirectToAction("Index", "HomeUser"); // ✅ Trỏ về HomeUser/Index
            }

            // View: Views/Account/LoginRegister.cshtml
            return View("LoginRegister");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string identifier, string password)
        {
            // identifier = email hoặc username
            if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View("LoginRegister");
            }

            // Hash mật khẩu
            var hashed = PasswordHasher.Hash(password);

            // Tìm user (Users table)
            var user = _context.Users.FirstOrDefault(u => u.Username == identifier || u.Email == identifier);
            if (user != null && PasswordHasher.Verify(password, user.PasswordHash))
            {
                // set session
                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                var role = _context.Roles.FirstOrDefault(r => r.RoleId == user.RoleId)?.RoleName ?? "";
                HttpContext.Session.SetString("RoleName", role);
                HttpContext.Session.SetString("Username", user.FullName ?? user.Username);

                if (role == "Admin") return RedirectToAction("Index", "Admin");
                if (role == "Staff") return RedirectToAction("Index", "Staff");
                return RedirectToAction("Index", "Home");
            }

            // Nếu không phải Users thì check Customers (email)
            var cust = _context.Customers.FirstOrDefault(c => c.Email == identifier);
            if (cust != null && PasswordHasher.Verify(password, cust.PasswordHash))
            {
                HttpContext.Session.SetString("UserId", cust.CustomerId.ToString());
                HttpContext.Session.SetString("RoleName", "Customer");
                HttpContext.Session.SetString("Username", cust.FullName ?? cust.Email);
                return RedirectToAction("Index", "HomeUser");
            }

            ViewBag.Error = "Sai tài khoản hoặc mật khẩu!";
            return View("LoginRegister");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string fullname, string email, string phone, string password, string address)
        {
            if (string.IsNullOrWhiteSpace(fullname) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Họ tên, email và mật khẩu là bắt buộc.";
                return View("LoginRegister");
            }

            // tránh duplicate email
            if (_context.Customers.Any(c => c.Email == email))
            {
                ViewBag.Error = "Email đã tồn tại!";
                return View("LoginRegister");
            }

            var hash = PasswordHasher.Hash(password);
            var customer = new Customer
            {
                FullName = fullname,
                Email = email,
                Phone = phone,
                PasswordHash = hash,
                Address = address
            };

            _context.Customers.Add(customer);
            _context.SaveChanges();

            ViewBag.Message = "Đăng ký thành công! Bạn có thể đăng nhập ngay.";
            return View("LoginRegister");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
