using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using WEBBANDIENTHOAI.Controllers;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Repositories;
using WEBBANDIENTHOAI.ViewModels;

namespace WEBBANDIENTHOAI.Tests.UnitTests.Controllers
{
    /// <summary>
    /// Test HomeUserController - Unit Test các chức năng chính của trang người dùng.
    /// Sử dụng Mock cho AppDbContext + Repository để test controller độc lập.
    /// </summary>
    [TestFixture]
    public class HomeUserControllerTests
    {
        private Mock<AppDbContext> _mockContext;           // Fake DbContext
        private Mock<ICustomerRepository> _mockCustomerRepo; // Fake Customer Repo
        private HomeUserController _controller;            // Controller chính cần test
        private Mock<ISession> _mockSession;               // Fake Session

        /// <summary>
        /// Chạy trước mỗi test – khởi tạo controller + fake session + TempData
        /// </summary>
        [SetUp]
        public void Setup()
        {
            // Tạo mock DbContext
            _mockContext = new Mock<AppDbContext>(new DbContextOptions<AppDbContext>());

            // Mock repository khách hàng
            _mockCustomerRepo = new Mock<ICustomerRepository>();

            // Inject mock vào controller
            _controller = new HomeUserController(_mockContext.Object, _mockCustomerRepo.Object);

            // Mock session
            _mockSession = new Mock<ISession>();
            var httpContext = new DefaultHttpContext();
            httpContext.Session = _mockSession.Object;

            // Gán HttpContext chứa session cho controller
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Mock TempData
            _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        }

        // ============================================================
        // =============== TEST INDEX – MÀN HÌNH CHÍNH ===============
        // ============================================================

        /// <summary>
        /// Nếu người dùng chưa đăng nhập => chuyển hướng Login
        /// </summary>
        [Test]
        public async Task Index_UserNotLoggedIn_RedirectsToLogin()
        {
            // Setup session rỗng (không có UserId và Role)
            SetupSession(null, null);

            var result = await _controller.Index();

            // Kiểm tra trả về RedirectToAction
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirect = result as RedirectToActionResult;
            Assert.That(redirect.ActionName, Is.EqualTo("Login"));
            Assert.That(redirect.ControllerName, Is.EqualTo("Account"));
        }

        /// <summary>
        /// Nếu đăng nhập nhưng không phải Customer => chuyển hướng Login
        /// </summary>
        [Test]
        public async Task Index_UserNotCustomerRole_RedirectsToLogin()
        {
            SetupSession("1", "Admin");

            var result = await _controller.Index();

            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirect = result as RedirectToActionResult;
            Assert.That(redirect.ActionName, Is.EqualTo("Login"));
            Assert.That(redirect.ControllerName, Is.EqualTo("Account"));
        }

        /// <summary>
        /// Nếu đăng nhập đúng role Customer => trả về View + load dữ liệu
        /// </summary>
        [Test]
        public async Task Index_UserLoggedInAsCustomer_ReturnsViewWithData()
        {
            var customer = new Customer
            {
                CustomerId = 1,
                FullName = "Test Customer",
                Email = "test@example.com",
                Phone = "0123456789",
                Address = "Test Address"
            };

            SetupSession("1", "Customer");

            // Mock repo trả về customer theo id
            _mockCustomerRepo.Setup(r => r.GetCustomerByIdAsync(1))
                             .ReturnsAsync(customer);

            // Mock danh sách sản phẩm
            var mockProducts = CreateMockDbSet(new List<Product>
            {
                new Product { ProductId = 1, CategoryId = 1, StatusId = 1, Price = 1000, Brand = "Apple" },
                new Product { ProductId = 2, CategoryId = 1, StatusId = 1, Price = 800,  Brand = "Samsung" },
                new Product { ProductId = 3, CategoryId = 2, StatusId = 1, Price = 1500, Brand = "Dell" },
                new Product { ProductId = 4, CategoryId = 2, StatusId = 1, Price = 1200, Brand = "HP" }
            });

            _mockContext.Setup(c => c.Products).Returns(mockProducts.Object);

            var result = await _controller.Index();

            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_controller.ViewBag.IsLoggedIn, Is.True);
            Assert.That(_controller.ViewBag.CustomerName, Is.EqualTo(customer.FullName));
        }

        // ============================================================
        // ============= TEST CHI TIẾT SẢN PHẨM =============
        // ============================================================

        /// <summary>
        /// Nếu sản phẩm không tồn tại => trả về 404
        /// </summary>
        [Test]
        public async Task ProductDetails_ProductNotExists_ReturnsNotFound()
        {
            _mockContext.Setup(c => c.Products)
                        .Returns(CreateMockDbSet(new List<Product>()).Object);

            var result = await _controller.ProductDetails(999);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        /// <summary>
        /// Nếu tồn tại => trả về View + model ProductDetailsVm
        /// </summary>
        [Test]
        public async Task ProductDetails_ProductExists_ReturnsViewWithViewModel()
        {
            var product = new Product
            {
                ProductId = 1,
                Name = "Test Product",
                CategoryId = 1,
                StatusId = 1,
                Price = 1000,
                Category = new Category { CategoryName = "Smartphone" },
                ProductStatus = new ProductStatus { StatusName = "Active" },
                PrimaryImage = new ProductImage { ImageId = 1 },
                Images = new List<ProductImage>()
            };

            _mockContext.Setup(c => c.Products)
                        .Returns(CreateMockDbSet(new List<Product> { product }).Object);

            var result = await _controller.ProductDetails(1);

            Assert.That(result, Is.InstanceOf<ViewResult>());

            var view = result as ViewResult;
            Assert.That(view.Model, Is.InstanceOf<ProductDetailsVm>());

            var vm = view.Model as ProductDetailsVm;
            Assert.That(vm.ProductId, Is.EqualTo(1));
            Assert.That(vm.Name, Is.EqualTo("Test Product"));
        }

        // ============================================================
        // ============= TEST SẢN PHẨM LIÊN QUAN =============
        // ============================================================

        /// <summary>
        /// Lấy sản phẩm liên quan => trả JsonResult
        /// </summary>
        [Test]
        public async Task GetRelatedProducts_ReturnsJsonWithRelatedProducts()
        {
            var products = new List<Product>
            {
                new Product { ProductId = 1, CategoryId = 1, StatusId = 1, Name = "Product 1", Price = 1000 },
                new Product { ProductId = 2, CategoryId = 1, StatusId = 1, Name = "Product 2", Price = 800  }
            };

            _mockContext.Setup(c => c.Products)
                        .Returns(CreateMockDbSet(products).Object);

            var result = await _controller.GetRelatedProducts(1, 1, 2);

            Assert.That(result, Is.InstanceOf<JsonResult>());
            Assert.That((result as JsonResult).Value, Is.Not.Null);
        }

        // ============================================================
        // =========== TEST LẤY ẢNH SẢN PHẨM ===============
        // ============================================================

        /// <summary>
        /// Ảnh không tồn tại => trả về ảnh mặc định
        /// </summary>
        [Test]
        public void GetProductImage_ImageNotExists_ReturnsDefaultImage()
        {
            _mockContext.Setup(c => c.ProductImages)
                        .Returns(CreateMockDbSet(new List<ProductImage>()).Object);

            var result = _controller.GetProductImage(999);

            Assert.That(result, Is.InstanceOf<FileResult>());
        }

        /// <summary>
        /// Ảnh tồn tại => trả về FileResult chứa byte[]
        /// </summary>
        [Test]
        public void GetProductImage_ImageExists_ReturnsImageFile()
        {
            var image = new ProductImage
            {
                ImageId = 1,
                ImagePath = new byte[] { 0x00, 0x01 }
            };

            _mockContext.Setup(c => c.ProductImages)
                        .Returns(CreateMockDbSet(new List<ProductImage> { image }).Object);

            var result = _controller.GetProductImage(1);

            Assert.That(result, Is.InstanceOf<FileResult>());
        }

        // ============================================================
        // =============== TEST BỘ LỌC SẢN PHẨM ===============
        // ============================================================

        /// <summary>
        /// Lọc smartphone theo hãng
        /// </summary>
        [Test]
        public async Task Smartphones_WithBrandFilter_ReturnsFilteredProducts()
        {
            var products = new List<Product>
            {
                new Product { ProductId = 1, CategoryId = 1, StatusId = 1, Brand = "Apple", Price = 1000 },
                new Product { ProductId = 2, CategoryId = 1, StatusId = 1, Brand = "Samsung", Price = 800  }
            };

            _mockContext.Setup(c => c.Products)
                        .Returns(CreateMockDbSet(products).Object);

            var result = await _controller.Smartphones("Apple", null);

            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That((result as ViewResult).Model, Is.Not.Null);
        }

        /// <summary>
        /// Lọc Laptop + sắp xếp theo giá tăng dần
        /// </summary>
        [Test]
        public async Task Laptops_WithSortFilter_ReturnsSortedProducts()
        {
            var products = new List<Product>
            {
                new Product { ProductId = 1, CategoryId = 2, StatusId = 1, Brand = "Dell", Price = 1000 },
                new Product { ProductId = 2, CategoryId = 2, StatusId = 1, Brand = "HP", Price = 800  }
            };

            _mockContext.Setup(c => c.Products)
                        .Returns(CreateMockDbSet(products).Object);

            var result = await _controller.Laptops(null, "price_asc");

            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That((result as ViewResult).Model, Is.Not.Null);
        }

        /// <summary>
        /// Test phân trang danh sách sản phẩm
        /// </summary>
        [Test]
        public async Task AllProducts_WithPagination_ReturnsPaginatedResults()
        {
            // Tạo 15 sản phẩm mẫu
            var products = new List<Product>();
            for (int i = 1; i <= 15; i++)
            {
                products.Add(new Product
                {
                    ProductId = i,
                    CategoryId = i % 2 + 1,
                    StatusId = 1,
                    Brand = "Brand" + (i % 3),
                    Price = i * 100
                });
            }

            _mockContext.Setup(c => c.Products)
                        .Returns(CreateMockDbSet(products).Object);

            var result = await _controller.AllProducts(null, null, 2);

            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_controller.ViewData["CurrentPage"], Is.EqualTo(2));
        }

        // ============================================================
        // ============= TEST SESSION TRONG PRODUCT DETAILS ===========
        // ============================================================

        /// <summary>
        /// Check session hiển thị thông tin khách hàng trong ProductDetails
        /// </summary>
        [Test]
        public async Task ProductDetails_UserLoggedIn_SetsCustomerInfoInViewBag()
        {
            var product = new Product
            {
                ProductId = 1,
                Name = "Test Product",
                CategoryId = 1,
                StatusId = 1,
                Category = new Category { CategoryName = "Smartphone" },
                ProductStatus = new ProductStatus { StatusName = "Active" },
                Images = new List<ProductImage>()
            };

            var customer = new Customer
            {
                CustomerId = 1,
                FullName = "Test Customer",
                Email = "test@example.com"
            };

            SetupSession("1", "Customer");

            _mockCustomerRepo.Setup(r => r.GetCustomerByIdAsync(1))
                             .ReturnsAsync(customer);

            _mockContext.Setup(c => c.Products)
                        .Returns(CreateMockDbSet(new List<Product> { product }).Object);

            var result = await _controller.ProductDetails(1);

            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_controller.ViewBag.IsLoggedIn, Is.True);
            Assert.That(_controller.ViewBag.CustomerName, Is.EqualTo(customer.FullName));
        }

        // ============================================================
        // =============== PRIVATE HELPER – MOCK SESSION ==============
        // ============================================================

        /// <summary>
        /// Set session UserId + RoleName
        /// </summary>
        private void SetupSession(string userId, string roleName)
        {
            var userIdBytes = userId != null ? System.Text.Encoding.UTF8.GetBytes(userId) : null;
            var roleBytes = roleName != null ? System.Text.Encoding.UTF8.GetBytes(roleName) : null;

            _mockSession.Setup(s => s.Get("UserId")).Returns(userIdBytes);
            _mockSession.Setup(s => s.Get("RoleName")).Returns(roleBytes);
        }

        // ============================================================
        // =============== PRIVATE HELPER – MOCK DBSET ================
        // ============================================================

        /// <summary>
        /// Mock DbSet<T> để hỗ trợ IQueryable LINQ: Where(), FirstOrDefault(), Count()
        /// </summary>
        private Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();

            // Setup IQueryable
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            return mockSet;
        }

        [TearDown]
        public void TearDown()
        {
            // Dọn controller sau mỗi test
            _controller?.Dispose();
        }
    }
}
