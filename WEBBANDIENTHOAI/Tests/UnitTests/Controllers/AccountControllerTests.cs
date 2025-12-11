using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using WEBBANDIENTHOAI.Controllers.TaiKhoan;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Helpers;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Services;

namespace WEBBANDIENTHOAI.Tests.UnitTests.Controllers
{
    [TestFixture]
    public class AccountControllerTests
    {
        private AccountController _controller;
        private AppDbContext _context;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IMailService> _mockMailService;
        private MockHttpSession _mockSession;

        // Custom Mock Session class
        public class MockHttpSession : ISession
        {
            private readonly Dictionary<string, byte[]> _sessionStorage = new Dictionary<string, byte[]>();

            public string Id => Guid.NewGuid().ToString();
            public bool IsAvailable => true;
            public IEnumerable<string> Keys => _sessionStorage.Keys;

            public void Clear() => _sessionStorage.Clear();

            public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

            public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

            public void Remove(string key) => _sessionStorage.Remove(key);

            public void Set(string key, byte[] value) => _sessionStorage[key] = value;

            public bool TryGetValue(string key, out byte[] value)
            {
                return _sessionStorage.TryGetValue(key, out value);
            }

            // Helper methods để dễ sử dụng
            public string GetString(string key)
            {
                return TryGetValue(key, out var value) ? System.Text.Encoding.UTF8.GetString(value) : null;
            }

            public void SetString(string key, string value)
            {
                Set(key, System.Text.Encoding.UTF8.GetBytes(value));
            }
        }

        [SetUp]
        public void Setup()
        {
            // Tạo InMemory database để test
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);

            // Mock configuration
            _mockConfiguration = new Mock<IConfiguration>();

            // Mock mail service
            _mockMailService = new Mock<IMailService>();
            _mockMailService.Setup(m => m.SendOTPEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);
            _mockMailService.Setup(m => m.SendPasswordResetSuccessAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(true);

            // Tạo controller với mock dependencies
            _controller = new AccountController(_context, _mockConfiguration.Object, _mockMailService.Object);

            // Tạo mock session custom
            _mockSession = new MockHttpSession();

            // Tạo HttpContext với mock session
            var httpContext = new DefaultHttpContext();
            httpContext.Session = _mockSession;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        // -------------------------------------------------------------
        // TEST 1: Đăng nhập với tài khoản hợp lệ (Customer)
        // -------------------------------------------------------------
        [Test]
        public async Task DangNhap_WithValidCustomerCredentials_ShouldRedirectToHomeUser()
        {
            // Arrange - Chuẩn bị dữ liệu test
            var customer = new Customer
            {
                Email = "testcustomer@example.com",
                PasswordHash = PasswordHasher.Hash("123456"),
                FullName = "Test Customer",
                Phone = "0123456789",
                CitizenID = "123456789012",
                Address = "123 Test Street",
                IsActive = true
            };
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();

            // Act - Thực hiện hành động test
            var result = await _controller.Login("testcustomer@example.com", "123456") as RedirectToActionResult;

            // Assert - Kiểm tra kết quả
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            Assert.That(result.ControllerName, Is.EqualTo("HomeUser"));

            // Kiểm tra session đã được set
            Assert.That(_mockSession.GetString("UserId"), Is.Not.Null);
            Assert.That(_mockSession.GetString("RoleName"), Is.EqualTo("Customer"));
        }

        // -------------------------------------------------------------
        // TEST 2: Đăng nhập với mật khẩu sai
        // -------------------------------------------------------------
        [Test]
        public async Task DangNhap_WithWrongPassword_ShouldReturnViewWithErrorMessage()
        {
            // Arrange
            var customer = new Customer
            {
                Email = "test@example.com",
                PasswordHash = PasswordHasher.Hash("correctpassword"),
                FullName = "Test User",
                Phone = "0123456789",
                CitizenID = "123456789012",
                Address = "123 Test Street",
                IsActive = true
            };
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Login("test@example.com", "wrongpassword") as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("LoginRegister"));
            Assert.That(_controller.ViewBag.Error, Is.Not.Null);
            Assert.That(_controller.ViewBag.Error.ToString(), Does.Contain("Sai tài khoản hoặc mật khẩu"));

            // Kiểm tra session không được set
            Assert.That(_mockSession.GetString("UserId"), Is.Null);
        }

        // -------------------------------------------------------------
        // TEST 3: Đăng nhập với email không tồn tại
        // -------------------------------------------------------------
        [Test]
        public async Task DangNhap_WithNonExistentEmail_ShouldReturnViewWithErrorMessage()
        {
            // Act
            var result = await _controller.Login("nonexistent@example.com", "anypassword") as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("LoginRegister"));
            Assert.That(_controller.ViewBag.Error, Is.Not.Null);
            Assert.That(_controller.ViewBag.Error.ToString(), Does.Contain("Sai tài khoản hoặc mật khẩu"));
        }

        // -------------------------------------------------------------
        // TEST 4: Đăng ký với dữ liệu hợp lệ
        // -------------------------------------------------------------
        [Test]
        public async Task DangKy_WithValidData_ShouldCreateNewCustomer()
        {
            // Arrange
            var fullname = "New Customer";
            var email = "newcustomer@example.com";
            var phone = "0123456789";
            var password = "123456";
            var address = "123 Test Street";

            // Act
            var result = await _controller.Register(fullname, email, phone, password, address) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("LoginRegister"));
            Assert.That(_controller.ViewBag.Message, Is.Not.Null);
            Assert.That(_controller.ViewBag.Message.ToString(), Does.Contain("Đăng ký thành công"));

            // Kiểm tra dữ liệu đã được lưu vào database
            var customerInDb = await _context.Customers.FirstOrDefaultAsync(c => c.Email == email);
            Assert.That(customerInDb, Is.Not.Null);
            Assert.That(customerInDb.FullName, Is.EqualTo(fullname));
            Assert.That(customerInDb.Phone, Is.EqualTo(phone));
            Assert.That(customerInDb.CitizenID.Length, Is.EqualTo(12)); // CitizenID phải có 12 ký tự
        }

        // -------------------------------------------------------------
        // TEST 5: Đăng ký với email đã tồn tại
        // -------------------------------------------------------------
        [Test]
        public async Task DangKy_WithDuplicateEmail_ShouldReturnErrorMessage()
        {
            // Arrange
            var existingCustomer = new Customer
            {
                Email = "existing@example.com",
                PasswordHash = new byte[32],
                FullName = "Existing Customer",
                Phone = "0123456789",
                CitizenID = "123456789012",
                Address = "456 Existing Ave",
                IsActive = true
            };
            await _context.Customers.AddAsync(existingCustomer);
            await _context.SaveChangesAsync();

            var fullname = "New Customer";
            var email = "existing@example.com"; // Email đã tồn tại
            var phone = "0987654321";
            var password = "123456";
            var address = "123 Test Street";

            // Act
            var result = await _controller.Register(fullname, email, phone, password, address) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("LoginRegister"));
            Assert.That(_controller.ViewBag.Error, Is.Not.Null);
            Assert.That(_controller.ViewBag.Error.ToString(), Does.Contain("Email đã tồn tại"));
        }

        // -------------------------------------------------------------
        // TEST 6: Đăng ký với số điện thoại không hợp lệ
        // -------------------------------------------------------------
        [Test]
        public async Task DangKy_WithInvalidPhoneNumber_ShouldReturnErrorMessage()
        {
            // Arrange
            var fullname = "Test User";
            var email = "test@example.com";
            var phone = "12345"; // Số điện thoại không hợp lệ
            var password = "123456";
            var address = "123 Test Street";

            // Act
            var result = await _controller.Register(fullname, email, phone, password, address) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("LoginRegister"));
            Assert.That(_controller.ViewBag.Error, Is.Not.Null);
            Assert.That(_controller.ViewBag.Error.ToString(), Does.Contain("Số điện thoại không đúng định dạng"));
        }

        // -------------------------------------------------------------
        // TEST 7: Đăng ký với mật khẩu quá ngắn (dưới 6 ký tự)
        // -------------------------------------------------------------
        [Test]
        public async Task DangKy_WithShortPassword_ShouldReturnErrorMessage()
        {
            // Arrange
            var fullname = "Test User";
            var email = "test@example.com";
            var phone = "0123456789";
            var password = "123"; // Mật khẩu quá ngắn
            var address = "123 Test Street";

            // Act
            var result = await _controller.Register(fullname, email, phone, password, address) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("LoginRegister"));
            Assert.That(_controller.ViewBag.Error, Is.Not.Null);
            Assert.That(_controller.ViewBag.Error.ToString(), Does.Contain("Mật khẩu phải có ít nhất 6 ký tự"));
        }

        // -------------------------------------------------------------
        // TEST 8: Kiểm tra tính bảo mật - Mật khẩu được hash
        // -------------------------------------------------------------
        [Test]
        public async Task DangKy_PasswordShouldBeHashed_NotStoredInPlainText()
        {
            // Arrange
            var password = "MySecurePassword123";
            var customer = new Customer
            {
                Email = "secure@example.com",
                PasswordHash = PasswordHasher.Hash(password),
                FullName = "Secure User",
                Phone = "0123456789",
                CitizenID = "123456789012",
                Address = "456 Secure Ave",
                IsActive = true
            };

            // Act
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();

            var savedCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == "secure@example.com");

            // Assert - Mật khẩu không được lưu dưới dạng plain text
            Assert.That(savedCustomer, Is.Not.Null);
            Assert.That(savedCustomer.PasswordHash, Is.Not.Null);

            // Kiểm tra byte array không phải là string plain text
            var passwordAsString = System.Text.Encoding.UTF8.GetString(savedCustomer.PasswordHash);
            Assert.That(passwordAsString, Is.Not.EqualTo(password));

            // Kiểm tra có thể verify được với mật khẩu đúng
            Assert.That(PasswordHasher.Verify(password, savedCustomer.PasswordHash), Is.True);

            // Kiểm tra không thể verify được với mật khẩu sai
            Assert.That(PasswordHasher.Verify("WrongPassword", savedCustomer.PasswordHash), Is.False);
        }

        // -------------------------------------------------------------
        // TEST 9: Quên mật khẩu với email hợp lệ
        // -------------------------------------------------------------
        [Test]
        public async Task QuenMatKhau_WithValidEmail_ShouldGenerateOTP()
        {
            // Arrange
            var customer = new Customer
            {
                Email = "forgot@example.com",
                PasswordHash = new byte[32],
                FullName = "Forgot User",
                Phone = "0123456789",
                CitizenID = "123456789012",
                Address = "789 Forgot St",
                IsActive = true
            };
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();

            var model = new ForgotPasswordModel
            {
                EmailOrPhone = "forgot@example.com"
            };

            // Act
            var result = await _controller.ForgotPassword(model) as RedirectToActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("VerifyOTP"));

            // Kiểm tra OTP đã được lưu vào database
            var otp = await _context.OTPCodes.FirstOrDefaultAsync(o => o.Email == "forgot@example.com");
            Assert.That(otp, Is.Not.Null);
            Assert.That(otp.Code.Length, Is.EqualTo(6)); // OTP phải có 6 chữ số
            Assert.That(otp.IsUsed, Is.False);
            Assert.That(otp.ExpiresAt, Is.GreaterThan(DateTime.UtcNow)); // Thời gian hết hạn phải lớn hơn hiện tại
        }

        // -------------------------------------------------------------
        // TEST 10: Kiểm tra OTP có thời gian hết hạn
        // -------------------------------------------------------------
        [Test]
        public async Task OTP_ShouldHaveExpirationTime()
        {
            // Arrange
            var otp = new OTPCode
            {
                Code = "123456",
                Email = "test@example.com",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10), // Hết hạn sau 10 phút
                IsUsed = false,
                Attempts = 0
            };

            // Act
            await _context.OTPCodes.AddAsync(otp);
            await _context.SaveChangesAsync();

            var savedOtp = await _context.OTPCodes.FirstOrDefaultAsync(o => o.Code == "123456");

            // Assert
            Assert.That(savedOtp, Is.Not.Null);
            Assert.That(savedOtp.CreatedAt, Is.LessThan(savedOtp.ExpiresAt)); // Thời gian tạo phải nhỏ hơn thời gian hết hạn
            Assert.That(savedOtp.ExpiresAt, Is.GreaterThan(DateTime.UtcNow)); // Chưa hết hạn tại thời điểm test
        }

        // -------------------------------------------------------------
        // TEST 11: Đăng xuất xóa session
        // -------------------------------------------------------------
        [Test]
        public void DangXuat_ShouldClearSession()
        {
            // Arrange - Set một số giá trị session
            _mockSession.SetString("UserId", "123");
            _mockSession.SetString("RoleName", "Customer");
            _mockSession.SetString("Username", "Test User");

            // Act
            var result = _controller.Logout() as RedirectToActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Login"));
            Assert.That(result.ControllerName, Is.EqualTo("Account"));

            // Kiểm tra session đã được clear
            Assert.That(_mockSession.Keys.Count(), Is.EqualTo(0));
        }

        // -------------------------------------------------------------
        // TEST 12: Kiểm tra email validation
        // -------------------------------------------------------------
        [Test]
        [TestCase("test@example.com", true)] // Email hợp lệ
        [TestCase("invalid-email", false)] // Email không hợp lệ
        [TestCase("test@domain", false)] // Email không hợp lệ
        [TestCase("", false)] // Email rỗng
        public void KiemTraEmail_ShouldValidateCorrectly(string email, bool expectedValid)
        {
            // Act
            // Sử dụng reflection để gọi private method IsValidEmail
            var method = typeof(AccountController).GetMethod("IsValidEmail",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var result = (bool)method.Invoke(_controller, new object[] { email });

            // Assert
            Assert.That(result, Is.EqualTo(expectedValid));
        }

        // -------------------------------------------------------------
        // TEST 13: Kiểm tra số điện thoại Việt Nam
        // -------------------------------------------------------------
        [Test]
        [TestCase("0123456789", true)] // Số điện thoại hợp lệ
        [TestCase("0912345678", true)] // Số điện thoại hợp lệ
        [TestCase("84123456789", false)] // Không bắt đầu bằng 0
        [TestCase("12345", false)] // Quá ngắn
        [TestCase("012345678901", false)] // Quá dài
        [TestCase("abc01234567", false)] // Chứa ký tự không phải số
        public void KiemTraSoDienThoai_ShouldValidateCorrectly(string phone, bool expectedValid)
        {
            // Act
            // Sử dụng reflection để gọi private method IsValidVietnamesePhoneNumber
            var method = typeof(AccountController).GetMethod("IsValidVietnamesePhoneNumber",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var result = (bool)method.Invoke(_controller, new object[] { phone });

            // Assert
            Assert.That(result, Is.EqualTo(expectedValid));
        }

        // -------------------------------------------------------------
        // TEST 14: Kiểm tra CitizenID được tạo đúng định dạng (12 số)
        // -------------------------------------------------------------
        [Test]
        public void TaoCitizenID_ShouldGenerate12DigitNumber()
        {
            // Act
            // Sử dụng reflection để gọi private method GenerateRandomCitizenId
            var method = typeof(AccountController).GetMethod("GenerateRandomCitizenId",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var citizenId = (string)method.Invoke(_controller, null);

            // Assert
            Assert.That(citizenId, Is.Not.Null);
            Assert.That(citizenId.Length, Is.EqualTo(12));

            // Kiểm tra tất cả ký tự đều là số
            foreach (char c in citizenId)
            {
                Assert.That(char.IsDigit(c), Is.True);
            }
        }

        // -------------------------------------------------------------
        // TEST 15: Đăng ký với dữ liệu trống
        // -------------------------------------------------------------
        [Test]
        public async Task DangKy_WithEmptyData_ShouldReturnErrorMessage()
        {
            // Arrange
            var fullname = ""; // Họ tên trống
            var email = ""; // Email trống
            var phone = "";
            var password = "";
            var address = "";

            // Act
            var result = await _controller.Register(fullname, email, phone, password, address) as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("LoginRegister"));
            Assert.That(_controller.ViewBag.Error, Is.Not.Null);
            Assert.That(_controller.ViewBag.Error.ToString(), Does.Contain("Họ tên, email và mật khẩu là bắt buộc"));
        }

        // -------------------------------------------------------------
        // TEST 16: Đăng nhập với dữ liệu trống
        // -------------------------------------------------------------
        [Test]
        public async Task DangNhap_WithEmptyCredentials_ShouldReturnErrorMessage()
        {
            // Act
            var result = await _controller.Login("", "") as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("LoginRegister"));
            Assert.That(_controller.ViewBag.Error, Is.Not.Null);
            Assert.That(_controller.ViewBag.Error.ToString(), Does.Contain("Vui lòng nhập đầy đủ thông tin"));
        }

        // -------------------------------------------------------------
        // TEST 17: Kiểm tra mật khẩu hash không bị null
        // -------------------------------------------------------------
        [Test]
        public void HashPassword_WithNullInput_ShouldReturnEmptyHash()
        {
            // Arrange
            string nullPassword = null;

            // Act
            var hash = SecurityHelper.HashPassword(nullPassword);

            // Assert
            Assert.That(hash, Is.Not.Null);
            Assert.That(hash.Length, Is.EqualTo(32)); // SHA256 hash có 32 bytes
        }

        // -------------------------------------------------------------
        // TEST 18: Kiểm tra convert byte array sang hex
        // -------------------------------------------------------------
        [Test]
        public void ToHex_ShouldConvertByteArrayToLowerCaseHex()
        {
            // Arrange
            byte[] testData = new byte[] { 0x12, 0x34, 0xAB, 0xCD };

            // Act
            var hex = testData.ToHex();

            // Assert
            Assert.That(hex, Is.EqualTo("1234abcd"));
            Assert.That(hex.Contains("-"), Is.False); // Không chứa dấu gạch ngang
        }

        
    }
}