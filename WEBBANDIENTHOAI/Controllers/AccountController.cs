using System.Linq;
using System.Text;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WEBBANDIENTHOAI.Models;

namespace WebBanDienThoai.Controllers
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
            return View("LoginRegister");
        }

        // Utility: hash plain password to SHA-256 bytes (match SQL HASHBYTES('SHA2_256', ...))
        private static byte[] HashPassword(string plain)
        {
            if (plain == null) plain = string.Empty;
            using var sha = SHA256.Create();
            return sha.ComputeHash(Encoding.UTF8.GetBytes(plain));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string usernameOrEmail, string password)
        {
            if (string.IsNullOrWhiteSpace(usernameOrEmail) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                return View("LoginRegister");
            }

            var hashed = HashPassword(password);

            // Tìm user (admin/staff) theo username hoặc email và so sánh PasswordHash
            var user = _context.Users
                .FirstOrDefault(u => (u.Username == usernameOrEmail || u.Email == usernameOrEmail));

            if (user != null)
            {
                var dbHash = user.PasswordHash ?? new byte[0];
                if (dbHash.SequenceEqual(hashed))
                {
                    HttpContext.Session.SetString("UserId", user.UserId.ToString());
                    var roleName = _context.Roles.FirstOrDefault(r => r.RoleId == user.RoleId)?.RoleName ?? "";
                    HttpContext.Session.SetString("RoleName", roleName);
                    HttpContext.Session.SetString("Username", user.FullName ?? user.Username);

                    if (roleName == "Admin")
                        return RedirectToAction("Index", "Admin");
                    else if (roleName == "Staff")
                        return RedirectToAction("Index", "Staff");
                    else
                        return RedirectToAction("Index", "Home");
                }
            }

            // Nếu không phải Users, kiểm tra Customers (theo Email)
            var customer = _context.Customers
                .FirstOrDefault(c => c.Email == usernameOrEmail);

            if (customer != null)
            {
                var custHash = customer.PasswordHash ?? new byte[0];
                if (custHash.SequenceEqual(hashed))
                {
                    HttpContext.Session.SetString("UserId", customer.CustomerId.ToString());
                    HttpContext.Session.SetString("RoleName", "Customer");
                    HttpContext.Session.SetString("Username", customer.FullName ?? customer.Email);
                    return RedirectToAction("Index", "Home");
                }
            }

            ViewBag.Error = "Sai tài khoản hoặc mật khẩu!";
            return View("LoginRegister");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string fullname, string email, string phone, string password, string address)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(fullname))
            {
                ViewBag.Error = "Họ tên, email và mật khẩu là bắt buộc.";
                return View("LoginRegister");
            }

            if (_context.Customers.Any(c => c.Email == email))
            {
                ViewBag.Error = "Email đã tồn tại!";
                return View("LoginRegister");
            }

            var hash = HashPassword(password);

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
