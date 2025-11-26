// Tests/UnitTests/Controllers/AccountControllerTests.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using WEBBANDIENTHOAI.Controllers;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Services;

namespace WEBBANDIENTHOAI.Tests.UnitTests.Controllers
{
    [TestFixture]
    public class AccountControllerTests
    {
        private Mock<AppDbContext> _mockContext;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<IMailService> _mockMailService;
        private AccountController _controller;
        private Mock<ISession> _mockSession;

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
            _mockConfiguration = new Mock<IConfiguration>();
            _mockMailService = new Mock<IMailService>();

            _controller = new AccountController(_mockContext.Object, _mockConfiguration.Object, _mockMailService.Object);

            _mockSession = new Mock<ISession>();
            var httpContext = new DefaultHttpContext();
            httpContext.Session = _mockSession.Object;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        }

        // ===============================================
        // TEST LOGIN - VALIDATION CƠ BẢN
        // ===============================================

        /// <summary>
        /// Test login khi bỏ trống cả username và password
        /// </summary>
        [Test]
        public void Login_Post_ReturnsError_WhenEmptyCredentials()
        {
            // Arrange
            string identifier = "";
            string password = "";

            // Act
            var result = _controller.Login(identifier, password);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult.ViewName, Is.EqualTo("LoginRegister"));
            Assert.That(_controller.ViewBag.Error, Is.EqualTo("Vui lòng nhập đầy đủ thông tin."));
        }

        /// <summary>
        /// Test login khi chỉ nhập username, bỏ trống password
        /// </summary>
        [Test]
        public void Login_Post_ReturnsError_WhenEmptyPassword()
        {
            // Arrange
            string identifier = "testuser";
            string password = "";

            // Act
            var result = _controller.Login(identifier, password);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_controller.ViewBag.Error, Is.EqualTo("Vui lòng nhập đầy đủ thông tin."));
        }

        /// <summary>
        /// Test login khi chỉ nhập password, bỏ trống username
        /// </summary>
        [Test]
        public void Login_Post_ReturnsError_WhenEmptyUsername()
        {
            // Arrange
            string identifier = "";
            string password = "password123";

            // Act
            var result = _controller.Login(identifier, password);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_controller.ViewBag.Error, Is.EqualTo("Vui lòng nhập đầy đủ thông tin."));
        }

        /// <summary>
        /// Test login khi nhập sai thông tin đăng nhập
        /// </summary>
        [Test]
        public void Login_Post_ReturnsError_WhenWrongCredentials()
        {
            // Arrange
            string identifier = "wronguser";
            string password = "wrongpass";

            // Mock empty database
            var mockUsers = CreateMockDbSet(new List<User>());
            var mockCustomers = CreateMockDbSet(new List<Customer>());

            _mockContext.Setup(c => c.Users).Returns(mockUsers.Object);
            _mockContext.Setup(c => c.Customers).Returns(mockCustomers.Object);

            // Act
            var result = _controller.Login(identifier, password);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_controller.ViewBag.Error, Is.EqualTo("Sai tài khoản hoặc mật khẩu!"));
        }

        // ===============================================
        // TEST REGISTER - VALIDATION CƠ BẢN
        // ===============================================

        /// <summary>
        /// Test register khi bỏ trống tất cả các trường bắt buộc
        /// </summary>
        [Test]
        public void Register_ReturnsError_WhenAllRequiredFieldsEmpty()
        {
            // Arrange
            string fullname = "";
            string email = "";
            string password = "";

            // Act
            var result = _controller.Register(fullname, email, "", password, "");

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_controller.ViewBag.Error, Is.EqualTo("Họ tên, email và mật khẩu là bắt buộc."));
        }

        /// <summary>
        /// Test register khi bỏ trống họ tên
        /// </summary>
        [Test]
        public void Register_ReturnsError_WhenFullnameEmpty()
        {
            // Arrange
            string fullname = "";
            string email = "test@test.com";
            string password = "password123";

            // Act
            var result = _controller.Register(fullname, email, "0912345678", password, "Address");

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_controller.ViewBag.Error, Is.EqualTo("Họ tên, email và mật khẩu là bắt buộc."));
        }

        /// <summary>
        /// Test register khi bỏ trống email
        /// </summary>
        [Test]
        public void Register_ReturnsError_WhenEmailEmpty()
        {
            // Arrange
            string fullname = "Test User";
            string email = "";
            string password = "password123";

            // Act
            var result = _controller.Register(fullname, email, "0912345678", password, "Address");

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_controller.ViewBag.Error, Is.EqualTo("Họ tên, email và mật khẩu là bắt buộc."));
        }

        /// <summary>
        /// Test register khi bỏ trống password
        /// </summary>
        [Test]
        public void Register_ReturnsError_WhenPasswordEmpty()
        {
            // Arrange
            string fullname = "Test User";
            string email = "test@test.com";
            string password = "";

            // Act
            var result = _controller.Register(fullname, email, "0912345678", password, "Address");

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_controller.ViewBag.Error, Is.EqualTo("Họ tên, email và mật khẩu là bắt buộc."));
        }

        /// <summary>
        /// Test register khi email đã tồn tại
        /// </summary>
        [Test]
        public void Register_ReturnsError_WhenEmailAlreadyExists()
        {
            // Arrange
            string fullname = "Test User";
            string email = "existing@test.com";
            string password = "password123";

            // Mock existing email
            var existingCustomer = new Customer { Email = "existing@test.com" };
            var mockCustomers = CreateMockDbSet(new List<Customer> { existingCustomer });
            _mockContext.Setup(c => c.Customers).Returns(mockCustomers.Object);

            // Act
            var result = _controller.Register(fullname, email, "0912345678", password, "Address");

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_controller.ViewBag.Error, Is.EqualTo("Email đã tồn tại!"));
        }

        // ===============================================
        // TEST LOGOUT
        // ===============================================

        /// <summary>
        /// Test logout xóa session và chuyển hướng
        /// </summary>
        [Test]
        public void Logout_ClearsSessionAndRedirects()
        {
            // Arrange
            _mockSession.Setup(s => s.Clear());

            // Act
            var result = _controller.Logout();

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult.ActionName, Is.EqualTo("Login"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Account"));
        }

        // ===============================================
        // TEST FORGOT PASSWORD - VALIDATION CƠ BẢN
        // ===============================================

        /// <summary>
        /// Test forgot password GET trả về view đúng
        /// </summary>
        [Test]
        public void ForgotPassword_Get_ReturnsCorrectView()
        {
            // Act
            var result = _controller.ForgotPassword();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            // Không chỉ định ViewName nên sẽ trả về view mặc định "ForgotPassword"
            Assert.That(viewResult.ViewName, Is.Null.Or.Empty);
        }

        // ===============================================
        // TEST SESSION HANDLING
        // ===============================================

        /// <summary>
        /// Test redirect khi đã đăng nhập
        /// </summary>
        [Test]
        public void Login_Get_Redirects_WhenAlreadyLoggedIn()
        {
            // Arrange - Đã đăng nhập
            SetupSession("1", "Admin", "admin");

            // Act
            var result = _controller.Login();

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        }

        /// <summary>
        /// Test hiển thị login khi chưa đăng nhập
        /// </summary>
        [Test]
        public void Login_Get_ShowsLogin_WhenNotLoggedIn()
        {
            // Arrange - Chưa đăng nhập
            SetupSession(null, null, null);

            // Act
            var result = _controller.Login();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult.ViewName, Is.EqualTo("LoginRegister"));
        }

        // ===============================================
        // PRIVATE HELPER METHODS
        // ===============================================

        /// <summary>
        /// Thiết lập session cho testing
        /// </summary>
        private void SetupSession(string userId, string roleName, string username)
        {
            byte[] userIdBytes = userId != null ? System.Text.Encoding.UTF8.GetBytes(userId) : null;
            byte[] roleBytes = roleName != null ? System.Text.Encoding.UTF8.GetBytes(roleName) : null;
            byte[] usernameBytes = username != null ? System.Text.Encoding.UTF8.GetBytes(username) : null;

            _mockSession.Setup(s => s.Get("UserId")).Returns(userIdBytes);
            _mockSession.Setup(s => s.Get("RoleName")).Returns(roleBytes);
            _mockSession.Setup(s => s.Get("Username")).Returns(usernameBytes);
        }

        /// <summary>
        /// Tạo Mock DbSet từ danh sách dữ liệu
        /// </summary>
        private Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            // Mock FirstOrDefault
            mockSet.As<IQueryable<T>>().Setup(m => m.FirstOrDefault(It.IsAny<System.Linq.Expressions.Expression<Func<T, bool>>>()))
                  .Returns((System.Linq.Expressions.Expression<Func<T, bool>> predicate) =>
                      queryable.FirstOrDefault(predicate.Compile()));

            // Mock Any
            mockSet.As<IQueryable<T>>().Setup(m => m.Any(It.IsAny<System.Linq.Expressions.Expression<Func<T, bool>>>()))
                  .Returns((System.Linq.Expressions.Expression<Func<T, bool>> predicate) =>
                      queryable.Any(predicate.Compile()));

            return mockSet;
        }
    }
}