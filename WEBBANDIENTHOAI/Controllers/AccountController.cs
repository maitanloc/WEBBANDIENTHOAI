using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Services;
using WEBBANDIENTHOAI.Models;
using Microsoft.Extensions.Configuration;

namespace WEBBANDIENTHOAI.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IMailService _mailService;

        public AccountController(AppDbContext context, IConfiguration configuration, IMailService mailService)
        {
            _context = context;
            _configuration = configuration;
            _mailService = mailService;
        }

        // ========== LOGIN ==========
        [HttpGet]
        public IActionResult Login()
        {
            // Nếu đã login -> redirect
            if (HttpContext.Session.GetString("UserId") != null)
            {
                var role = HttpContext.Session.GetString("RoleName") ?? "";
                if (role == "Admin") return RedirectToAction("Index", "Admin");
                if (role == "Staff") return RedirectToAction("Index", "Staff");
                return RedirectToAction("Index", "HomeUser");
            }

            return View("LoginRegister");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string identifier, string password)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(password))
                {
                    ViewBag.Error = "Vui lòng nhập đầy đủ thông tin.";
                    return View("LoginRegister");
                }

                // Chuẩn hóa identifier
                identifier = identifier.Trim().ToLower();

                // Tìm user (Users table)
                var user = _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefault(u => u.Username == identifier || u.Email == identifier);

                if (user != null && user.PasswordHash != null && PasswordHasher.Verify(password, user.PasswordHash))
                {
                    // set session
                    HttpContext.Session.SetString("UserId", user.UserId.ToString());
                    var role = user.Role?.RoleName ?? "";
                    HttpContext.Session.SetString("RoleName", role);
                    HttpContext.Session.SetString("Username", !string.IsNullOrEmpty(user.FullName) ? user.FullName : user.Username);

                    if (role == "Admin") return RedirectToAction("Index", "Admin");
                    if (role == "Staff") return RedirectToAction("Index", "Staff");
                    return RedirectToAction("Index", "Home");
                }

                // Tìm customer
                var cust = _context.Customers.FirstOrDefault(c =>
                    c.Email == identifier ||
                    (!string.IsNullOrEmpty(c.Phone) && c.Phone == identifier));

                if (cust != null && cust.PasswordHash != null && PasswordHasher.Verify(password, cust.PasswordHash))
                {
                    HttpContext.Session.SetString("UserId", cust.CustomerId.ToString());
                    HttpContext.Session.SetString("RoleName", "Customer");
                    HttpContext.Session.SetString("Username", !string.IsNullOrEmpty(cust.FullName) ? cust.FullName : cust.Email);
                    return RedirectToAction("Index", "HomeUser");
                }

                ViewBag.Error = "Sai tài khoản hoặc mật khẩu!";
                return View("LoginRegister");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Login error: {ex.Message}");
                ViewBag.Error = "Có lỗi xảy ra khi đăng nhập. Vui lòng thử lại.";
                return View("LoginRegister");
            }
        }

        // ========== REGISTER ==========
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(string fullname, string email, string phone, string password, string address)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fullname) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    ViewBag.Error = "Họ tên, email và mật khẩu là bắt buộc.";
                    return View("LoginRegister");
                }

                // Kiểm tra email đã tồn tại
                if (_context.Customers.Any(c => c.Email == email))
                {
                    ViewBag.Error = "Email đã tồn tại!";
                    return View("LoginRegister");
                }

                // Tạo CitizenID ngẫu nhiên
                string citizenId;
                do
                {
                    citizenId = GenerateRandomCitizenId();
                } while (_context.Customers.Any(c => c.CitizenID == citizenId));

                // Tạo customer mới
                var customer = new Customer
                {
                    FullName = fullname?.Trim(),
                    Email = email?.Trim().ToLower(),
                    Phone = phone?.Trim(),
                    PasswordHash = PasswordHasher.Hash(password),
                    CitizenID = citizenId,
                    Address = address?.Trim(),
                    IsActive = true
                };

                _context.Customers.Add(customer);
                _context.SaveChanges();

                ViewBag.Message = "Đăng ký thành công! Bạn có thể đăng nhập ngay.";
                return View("LoginRegister");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Register error: {ex.Message}");
                ViewBag.Error = $"Lỗi đăng ký: {ex.Message}";
                return View("LoginRegister");
            }
        }

        // ========== FORGOT PASSWORD ==========
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            try
            {
                Console.WriteLine($"=== FORGOT PASSWORD STARTED ===");

                if (!ModelState.IsValid)
                {
                    ViewBag.Error = "Vui lòng nhập email hợp lệ.";
                    return View(model);
                }

                var email = model.EmailOrPhone.Trim().ToLower();
                Console.WriteLine($"Processing email: {email}");

                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == email);

                if (customer == null)
                {
                    Console.WriteLine($"Customer not found for email: {email}");
                    ViewBag.Success = "Nếu email tồn tại trong hệ thống, chúng tôi đã gửi mã OTP. Vui lòng kiểm tra hộp thư.";
                    return View();
                }

                Console.WriteLine($"Customer found: {customer.FullName}");

                // Tạo mã OTP
                var random = new Random();
                var otpCode = random.Next(100000, 999999).ToString();
                Console.WriteLine($"Generated OTP: {otpCode}");

                // Xóa OTP cũ
                var oldOtps = _context.OTPCodes.Where(o => o.Email == email);
                _context.OTPCodes.RemoveRange(oldOtps);

                // Lưu OTP mới
                var otpEntity = new OTPCode
                {
                    Code = otpCode,
                    Email = email,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    IsUsed = false,
                    Attempts = 0
                };

                _context.OTPCodes.Add(otpEntity);
                await _context.SaveChangesAsync();
                Console.WriteLine("OTP saved to database");

                // Gửi email
                var emailSent = await _mailService.SendOTPEmailAsync(email, otpCode, customer.FullName);
                Console.WriteLine($"Email sent: {emailSent}");

                if (emailSent)
                {
                    Console.WriteLine($"Redirecting to VerifyOTP with email: {email}");
                    return RedirectToAction("VerifyOTP", new { email = email });
                }
                else
                {
                    ViewBag.Error = "Có lỗi khi gửi email. Vui lòng thử lại sau.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ForgotPassword error: {ex.Message}");
                ViewBag.Error = "Có lỗi xảy ra. Vui lòng thử lại sau.";
                return View(model);
            }
        }

        // ========== VERIFY OTP ==========
        [HttpGet]
        public IActionResult VerifyOTP(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Email không hợp lệ.";
                return RedirectToAction("ForgotPassword");
            }

            var model = new VerifyOTPModel { Email = email };

            // Lấy thông tin OTP hiện tại để hiển thị số lần thử còn lại
            var currentOtp = _context.OTPCodes
                .Where(o => o.Email == email && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefault();

            if (currentOtp != null)
            {
                ViewBag.AttemptsLeft = 5 - currentOtp.Attempts;
                ViewBag.OtpExpiresAt = currentOtp.ExpiresAt;
            }
            else
            {
                ViewBag.AttemptsLeft = 5;
                ViewBag.OtpExpiresAt = null;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOTP(VerifyOTPModel model)
        {
            try
            {
                Console.WriteLine($"=== VERIFY OTP STARTED ===");
                Console.WriteLine($"Email: {model.Email}");
                Console.WriteLine($"OTP: {model.OTP}");

                if (!ModelState.IsValid)
                {
                    Console.WriteLine($"ModelState invalid: {string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                    ViewBag.Error = "Vui lòng nhập mã OTP hợp lệ (6 chữ số).";
                    return View(model);
                }

                // Tìm OTP hợp lệ
                var validOtp = await _context.OTPCodes
                    .FirstOrDefaultAsync(o => o.Email == model.Email
                                           && o.Code == model.OTP
                                           && !o.IsUsed
                                           && o.ExpiresAt > DateTime.UtcNow);

                if (validOtp != null)
                {
                    // OTP đúng
                    Console.WriteLine("OTP is valid");

                    // Đánh dấu OTP đã sử dụng
                    validOtp.IsUsed = true;
                    await _context.SaveChangesAsync();

                    // Tạo token reset password
                    var token = Guid.NewGuid().ToString();
                    Console.WriteLine($"Generated token: {token}");

                    // Xóa các token cũ
                    var oldTokens = _context.PasswordResetTokens.Where(t => t.Email == model.Email);
                    _context.PasswordResetTokens.RemoveRange(oldTokens);

                    // Lưu token mới
                    var resetToken = new PasswordResetToken
                    {
                        Token = token,
                        Email = model.Email,
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                        IsUsed = false
                    };

                    _context.PasswordResetTokens.Add(resetToken);
                    await _context.SaveChangesAsync();

                    Console.WriteLine("Redirecting to ResetPassword...");
                    return RedirectToAction("ResetPassword", new { token = token });
                }
                else
                {
                    // OTP sai - tìm OTP hiện tại
                    var currentOtp = await _context.OTPCodes
                        .Where(o => o.Email == model.Email && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
                        .OrderByDescending(o => o.CreatedAt)
                        .FirstOrDefaultAsync();

                    if (currentOtp != null)
                    {
                        // Tăng số lần thử sai
                        currentOtp.Attempts++;
                        await _context.SaveChangesAsync();

                        int attemptsLeft = 5 - currentOtp.Attempts;
                        ViewBag.AttemptsLeft = attemptsLeft;

                        if (currentOtp.Attempts >= 5)
                        {
                            // Đã vượt quá 5 lần thử
                            currentOtp.IsUsed = true;
                            await _context.SaveChangesAsync();

                            ViewBag.Error = "Bạn đã nhập sai OTP quá 5 lần. Vui lòng yêu cầu mã mới.";
                        }
                        else
                        {
                            ViewBag.Error = $"Mã OTP không đúng. Bạn còn {attemptsLeft} lần thử.";
                        }
                    }
                    else
                    {
                        ViewBag.Error = "Mã OTP không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu mã mới.";
                        ViewBag.AttemptsLeft = 0;
                    }

                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== VERIFY OTP ERROR ===");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                Console.WriteLine($"=== END ERROR ===");

                ViewBag.Error = "Có lỗi xảy ra khi xác minh OTP. Vui lòng thử lại.";
                return View(model);
            }
        }

        // ========== RESEND OTP ==========
        [HttpGet]
        public IActionResult ResendOTP()
        {
            return RedirectToAction("ForgotPassword");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOTP(string email)
        {
            try
            {
                Console.WriteLine($"=== RESEND OTP STARTED ===");
                Console.WriteLine($"Email: {email}");

                if (string.IsNullOrEmpty(email))
                {
                    TempData["Error"] = "Email không hợp lệ.";
                    return RedirectToAction("VerifyOTP", new { email = email });
                }

                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == email);

                if (customer == null)
                {
                    // Bảo mật: không cho biết email có tồn tại không
                    TempData["Success"] = "Nếu email tồn tại trong hệ thống, mã OTP mới đã được gửi.";
                    return RedirectToAction("VerifyOTP", new { email = email });
                }

                // Xóa các OTP cũ
                var oldOtps = _context.OTPCodes.Where(o => o.Email == email);
                _context.OTPCodes.RemoveRange(oldOtps);

                // Tạo mã OTP mới
                var random = new Random();
                var otpCode = random.Next(100000, 999999).ToString();
                Console.WriteLine($"New OTP generated: {otpCode}");

                // Lưu OTP mới
                var otpEntity = new OTPCode
                {
                    Code = otpCode,
                    Email = email,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10),
                    IsUsed = false,
                    Attempts = 0
                };

                _context.OTPCodes.Add(otpEntity);
                await _context.SaveChangesAsync();
                Console.WriteLine("New OTP saved to database");

                // Gửi email OTP mới
                var emailSent = await _mailService.SendOTPEmailAsync(email, otpCode, customer.FullName);
                Console.WriteLine($"Email sent: {emailSent}");

                if (emailSent)
                {
                    TempData["Success"] = "Đã gửi lại mã OTP mới. Vui lòng kiểm tra email.";
                }
                else
                {
                    TempData["Error"] = "Có lỗi khi gửi email. Vui lòng thử lại.";
                }

                return RedirectToAction("VerifyOTP", new { email = email });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ResendOTP error: {ex.Message}");
                TempData["Error"] = "Có lỗi khi gửi lại mã OTP. Vui lòng thử lại.";
                return RedirectToAction("VerifyOTP", new { email = email });
            }
        }

        // ========== RESET PASSWORD ==========
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Token không hợp lệ.";
                return RedirectToAction("ForgotPassword");
            }

            // Kiểm tra token hợp lệ
            var validToken = _context.PasswordResetTokens
                .FirstOrDefault(t => t.Token == token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);

            if (validToken == null)
            {
                TempData["Error"] = "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction("ForgotPassword");
            }

            var model = new ResetPasswordModel { Token = token };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            try
            {
                Console.WriteLine($"=== RESET PASSWORD STARTED ===");
                Console.WriteLine($"Token: {model.Token}");

                if (!ModelState.IsValid)
                {
                    Console.WriteLine($"ModelState invalid: {string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                    ViewBag.Error = "Vui lòng kiểm tra lại thông tin nhập.";
                    return View(model);
                }

                // Kiểm tra token
                var validToken = await _context.PasswordResetTokens
                    .FirstOrDefaultAsync(t => t.Token == model.Token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);

                Console.WriteLine($"Valid token found: {validToken != null}");

                if (validToken == null)
                {
                    ViewBag.Error = "Liên kết đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.";
                    return View(model);
                }

                // Tìm customer
                var customer = await _context.Customers
                    .FirstOrDefaultAsync(c => c.Email == validToken.Email);

                Console.WriteLine($"Customer found: {customer != null}");

                if (customer == null)
                {
                    ViewBag.Error = "Không tìm thấy thông tin người dùng.";
                    return View(model);
                }

                // Cập nhật mật khẩu
                customer.PasswordHash = PasswordHasher.Hash(model.NewPassword);

                // Đánh dấu token đã sử dụng
                validToken.IsUsed = true;

                await _context.SaveChangesAsync();
                Console.WriteLine("Password updated successfully");

                // Gửi email thông báo thành công
                var emailSent = await _mailService.SendPasswordResetSuccessAsync(customer.Email, customer.FullName);
                Console.WriteLine($"Success email sent: {emailSent}");

                TempData["Success"] = "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập bằng mật khẩu mới.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== RESET PASSWORD ERROR ===");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                Console.WriteLine($"=== END ERROR ===");

                ViewBag.Error = "Có lỗi xảy ra khi đặt lại mật khẩu. Vui lòng thử lại.";
                return View(model);
            }
        }

        // ========== LOGOUT ==========
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        // ========== UTILITY METHODS ==========
        private string GenerateRandomCitizenId()
        {
            var random = new Random();
            var citizenId = new char[12];

            for (int i = 0; i < 12; i++)
            {
                citizenId[i] = (char)('0' + random.Next(0, 10));
            }

            return new string(citizenId);
        }
    }
}