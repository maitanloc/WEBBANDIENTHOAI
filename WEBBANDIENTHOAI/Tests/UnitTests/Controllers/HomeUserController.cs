using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WEBBANDIENTHOAI.Controllers.NguoiDung;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Repository.TaiKhoan;
using System.Collections.Generic;

namespace WEBBANDIENTHOAI.Tests.Controllers
{
    [TestFixture]
    public class HomeUserControllerTests
    {
        private Mock<AppDbContext> _mockContext;
        private Mock<ICustomerRepository> _mockCustomerRepository;
        private HomeUserController _controller;
        private Mock<HttpContext> _mockHttpContext;
        private Mock<ISession> _mockSession;

        [SetUp]
        public void Setup()
        {
            _mockContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());
            _mockCustomerRepository = new Mock<ICustomerRepository>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockSession = new Mock<ISession>();

            _mockHttpContext.Setup(c => c.Session).Returns(_mockSession.Object);

            _controller = new HomeUserController(_mockContext.Object, _mockCustomerRepository.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                }
            };
        }

        // Test 1: Kiểm tra khi chưa đăng nhập
        [Test]
        public async Task Index_ChuyenHuongDen_Login_Khi_ChuaDangNhap()
        {
            // Arrange
            byte[] userIdBytes = null;
            byte[] roleBytes = null;

            _mockSession.Setup(s => s.TryGetValue("UserId", out userIdBytes)).Returns(false);
            _mockSession.Setup(s => s.TryGetValue("RoleName", out roleBytes)).Returns(false);

            // Act
            var result = await _controller.Index() as RedirectToActionResult;

            // Assert - Sử dụng Assert.Multiple cho nhiều kiểm tra
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.ActionName, Is.EqualTo("Login"));
                Assert.That(result.ControllerName, Is.EqualTo("Account"));
            });
        }

        // Test 2: Kiểm tra khi role không phải Customer
        [Test]
        public async Task Index_ChuyenHuongDen_Login_Khi_Role_KhongPhaiCustomer()
        {
            // Arrange
            var userIdBytes = System.Text.Encoding.UTF8.GetBytes("1");
            var roleBytes = System.Text.Encoding.UTF8.GetBytes("Admin");

            _mockSession.Setup(s => s.TryGetValue("UserId", out userIdBytes)).Returns(true);
            _mockSession.Setup(s => s.TryGetValue("RoleName", out roleBytes)).Returns(true);

            // Act
            var result = await _controller.Index() as RedirectToActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ActionName, Is.EqualTo("Login"));
        }

        // Test 3: Kiểm tra ProductDetails với ID không tồn tại
        [Test]
        public async Task ProductDetails_TraVe_NotFound_Khi_SanPham_KhongTonTai()
        {
            // Arrange
            var productId = 999;
            var products = new List<Product>().AsQueryable();
            var mockSet = CreateMockDbSet(products);

            _mockContext.Setup(c => c.Products).Returns(mockSet.Object);

            // Act
            var result = await _controller.ProductDetails(productId);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        // Test 4: Kiểm tra Search với keyword rỗng
        [Test]
        public async Task Search_TraVe_View_Voi_DanhSachRong_Khi_KeywordRong()
        {
            // Arrange
            var keyword = "";
            var products = new List<Product>().AsQueryable();
            var mockSet = CreateMockDbSet(products);

            _mockContext.Setup(c => c.Products).Returns(mockSet.Object);

            // Act
            var result = await _controller.Search(keyword) as ViewResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.ViewData["Keyword"], Is.EqualTo(keyword));
                Assert.That(result.Model, Is.InstanceOf<List<Product>>());
                Assert.That((result.Model as List<Product>)!.Count, Is.EqualTo(0));
            });
        }

        // Test 5: Kiểm tra GetProductImage với ImageId không tồn tại
        [Test]
        public void GetProductImage_TraVe_FileMacDinh_Khi_Image_KhongTonTai()
        {
            // Arrange
            var imageId = 999;
            var productImages = new List<ProductImage>().AsQueryable();
            var mockSet = CreateMockDbSet(productImages);

            _mockContext.Setup(c => c.ProductImages).Returns(mockSet.Object);

            // Act
            var result = _controller.GetProductImage(imageId) as FileResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.ContentType, Is.EqualTo("image/png"));
        }

        // Test 6: Kiểm tra Smartphones với brand không tồn tại
        [Test]
        public async Task Smartphones_TraVe_DanhSachRong_Khi_Brand_KhongTonTai()
        {
            // Arrange
            var brand = "BrandKhongTonTai";
            var sort = "price_asc";

            var products = new List<Product>().AsQueryable();
            var mockSet = CreateMockDbSet(products);

            _mockContext.Setup(c => c.Products).Returns(mockSet.Object);

            // Act
            var result = await _controller.Smartphones(brand, sort) as ViewResult;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result!.Model, Is.InstanceOf<List<Product>>());
                Assert.That((result.Model as List<Product>)!.Count, Is.EqualTo(0));
            });
        }

        // Helper method để tạo Mock DbSet
        private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }
    }
}