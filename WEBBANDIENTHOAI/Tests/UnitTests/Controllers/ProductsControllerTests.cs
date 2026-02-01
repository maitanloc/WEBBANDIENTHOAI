// Tests/UnitTests/Controllers/ProductsControllerTests.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Controllers.Admin;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Repository.Admin;
using WEBBANDIENTHOAI.ViewModels;

namespace WEBBANDIENTHOAI.Tests.UnitTests.Controllers
{
    [TestFixture]
    public class ProductsControllerTests
    {
        private Mock<IProductRepository> _mockProductRepo;
        private Mock<ILogger<ProductsController>> _mockLogger;
        private AppDbContext _context;
        private ProductsController _controller;

        [SetUp]
        public void Setup()
        {
            _mockProductRepo = new Mock<IProductRepository>();
            _mockLogger = new Mock<ILogger<ProductsController>>();

            // Tạo database trong bộ nhớ để test
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid().ToString())
                .Options;
            _context = new AppDbContext(options);

            _controller = new ProductsController(_mockProductRepo.Object, _mockLogger.Object, _context);

            // Cấu hình TempData cho controller
            _controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // ===============================================
        // TEST ACTION INDEX
        // ===============================================

        /// <summary>
        /// Test trang danh sách sản phẩm trả về view với dữ liệu
        /// </summary>
        [Test]
        public async Task Index_ReturnsViewWithProducts()
        {
            // Arrange
            var mockProducts = new List<ProductListItemVm> {
                new ProductListItemVm { ProductId = 1, Name = "iPhone 15" },
                new ProductListItemVm { ProductId = 2, Name = "Samsung Galaxy" }
            };

            _mockProductRepo.Setup(repo => repo.GetFilteredAsync(null, null, null, null, null, null, null, 1, 20))
                .ReturnsAsync((mockProducts, 2));

            // Act
            var result = await _controller.Index(null, null, null, null, null, null, null, 1, 20);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult.Model, Is.InstanceOf<IEnumerable<ProductListItemVm>>());
            Assert.That(viewResult.ViewData["Title"], Is.EqualTo("Sản phẩm"));
        }

        /// <summary>
        /// Test trang danh sách sản phẩm với bộ lọc
        /// </summary>
        [Test]
        public async Task Index_WithFilters_ReturnsFilteredProducts()
        {
            // Arrange
            var mockProducts = new List<ProductListItemVm> {
                new ProductListItemVm { ProductId = 1, Name = "iPhone 15" }
            };

            _mockProductRepo.Setup(repo => repo.GetFilteredAsync("iPhone", 1, 1, 10000000m, 30000000m, true, "price_desc", 1, 20))
                .ReturnsAsync((mockProducts, 1));

            // Act
            var result = await _controller.Index("iPhone", 1, 1, 10000000m, 30000000m, true, "price_desc", 1, 20);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            var model = viewResult.Model as IEnumerable<ProductListItemVm>;
            Assert.That(model.Count(), Is.EqualTo(1));
        }

        /// <summary>
        /// Test trang danh sách sản phẩm trả về partial view khi request AJAX
        /// </summary>
        [Test]
        public async Task Index_WithAjaxRequest_ReturnsPartialView()
        {
            // Arrange
            var mockProducts = new List<ProductListItemVm> {
                new ProductListItemVm { ProductId = 1, Name = "iPhone 15" }
            };

            _mockProductRepo.Setup(repo => repo.GetFilteredAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<byte?>(),
                It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<bool?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync((mockProducts, 1));

            // Mock AJAX request
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request = { Headers = { ["X-Requested-With"] = "XMLHttpRequest" } }
                }
            };

            // Act
            var result = await _controller.Index(null, null, null, null, null, null, null, 1, 20);

            // Assert
            Assert.That(result, Is.InstanceOf<PartialViewResult>());
            var partialResult = result as PartialViewResult;
            Assert.That(partialResult.ViewName, Is.EqualTo("_ProductsIndexPartial"));
        }

        /// <summary>
        /// Test xử lý lỗi khi load danh sách sản phẩm
        /// </summary>
        [Test]
        public async Task Index_WhenException_ReturnsEmptyViewWithError()
        {
            // Arrange
            _mockProductRepo.Setup(repo => repo.GetFilteredAsync(It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<byte?>(),
                It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<bool?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.Index(null, null, null, null, null, null, null, 1, 20);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            var model = viewResult.Model as IEnumerable<ProductListItemVm>;
            Assert.That(model.Count(), Is.EqualTo(0));
            Assert.That(_controller.TempData["Error"], Is.Not.Null);
        }

        // ===============================================
        // TEST ACTION DETAILS
        // ===============================================

        /// <summary>
        /// Test chi tiết sản phẩm với ID null trả về NotFound
        /// </summary>
        [Test]
        public async Task Details_WithNullId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Details(null);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        /// <summary>
        /// Test chi tiết sản phẩm với ID không tồn tại trả về NotFound
        /// </summary>
        [Test]
        public async Task Details_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            _mockProductRepo.Setup(repo => repo.GetByIdWithIncludesAsync(It.IsAny<int>()))
                .ReturnsAsync((Product)null);

            // Act
            var result = await _controller.Details(999);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        /// <summary>
        /// Test chi tiết sản phẩm với ID hợp lệ trả về view với dữ liệu
        /// </summary>
        [Test]
        public async Task Details_WithValidId_ReturnsViewWithProduct()
        {
            // Arrange
            var product = new Product
            {
                ProductId = 1,
                Name = "iPhone 15",
                Inventory = new List<Inventory>(),
                ExportDetails = new List<ExportReceiptDetail>(),
                ImportDetails = new List<ImportReceiptDetail>()
            };

            _mockProductRepo.Setup(repo => repo.GetByIdWithIncludesAsync(1))
                .ReturnsAsync(product);

            // Act
            var result = await _controller.Details(1);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult.Model, Is.InstanceOf<Product>());
            Assert.That(viewResult.ViewData["CanEdit"], Is.Not.Null);
        }

        /// <summary>
        /// Test xử lý lỗi khi load chi tiết sản phẩm
        /// </summary>
        [Test]
        public async Task Details_WhenException_RedirectsWithError()
        {
            // Arrange
            _mockProductRepo.Setup(repo => repo.GetByIdWithIncludesAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            var result = await _controller.Details(1);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
            Assert.That(_controller.TempData["Error"], Is.Not.Null);
        }

        // ===============================================
        // TEST ACTION EDIT (GET)
        // ===============================================

        /// <summary>
        /// Test trang edit với ID null trả về NotFound
        /// </summary>
        [Test]
        public async Task Edit_Get_WithNullId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Edit(null);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        /// <summary>
        /// Test trang edit với ID không tồn tại trả về NotFound
        /// </summary>
        [Test]
        public async Task Edit_Get_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            _mockProductRepo.Setup(repo => repo.GetByIdWithIncludesAsync(It.IsAny<int>()))
                .ReturnsAsync((Product)null);

            // Act
            var result = await _controller.Edit(999);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        /// <summary>
        /// Test trang edit khi sản phẩm không thể chỉnh sửa redirect với lỗi
        /// </summary>
        [Test]
        public async Task Edit_Get_WhenProductNotEditable_RedirectsWithError()
        {
            // Arrange
            var product = new Product
            {
                ProductId = 1,
                Name = "iPhone 15",
                Inventory = new List<Inventory> { new Inventory() }, // Có inventory nên không thể edit
                ExportDetails = new List<ExportReceiptDetail>(),
                ImportDetails = new List<ImportReceiptDetail>()
            };

            _mockProductRepo.Setup(repo => repo.GetByIdWithIncludesAsync(1))
                .ReturnsAsync(product);

            // Act
            var result = await _controller.Edit(1);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult.ActionName, Is.EqualTo("Details"));
            Assert.That(_controller.TempData["Error"], Is.Not.Null);
        }

        /// <summary>
        /// Test trang edit với sản phẩm có thể chỉnh sửa trả về view
        /// </summary>
        [Test]
        public async Task Edit_Get_WithEditableProduct_ReturnsView()
        {
            // Arrange
            var product = new Product
            {
                ProductId = 1,
                Name = "iPhone 15",
                Inventory = new List<Inventory>(), // Không có inventory nên có thể edit
                ExportDetails = new List<ExportReceiptDetail>(),
                ImportDetails = new List<ImportReceiptDetail>()
            };

            _mockProductRepo.Setup(repo => repo.GetByIdWithIncludesAsync(1))
                .ReturnsAsync(product);

            _mockProductRepo.Setup(repo => repo.GetAllStatusesAsync())
                .ReturnsAsync(new List<ProductStatus>());
            _mockProductRepo.Setup(repo => repo.GetAllCategoriesAsync())
                .ReturnsAsync(new List<Category>());

            // Act
            var result = await _controller.Edit(1);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult.Model, Is.InstanceOf<Product>());
            Assert.That(viewResult.ViewData["StatusId"], Is.InstanceOf<SelectList>());
            Assert.That(viewResult.ViewData["CategoryId"], Is.InstanceOf<SelectList>());
        }

        // ===============================================
        // TEST ACTION EDIT (POST)
        // ===============================================

        /// <summary>
        /// Test edit post với ID không khớp trả về NotFound
        /// </summary>
        [Test]
        public async Task Edit_Post_WithIdMismatch_ReturnsNotFound()
        {
            // Arrange
            var product = new Product { ProductId = 2 };

            // Act
            var result = await _controller.Edit(1, product, null);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        /// <summary>
        /// Test edit post với model không hợp lệ trả về view
        /// </summary>
        [Test]
        public async Task Edit_Post_WithInvalidModel_ReturnsView()
        {
            // Arrange
            var product = new Product { ProductId = 1, Name = "Test" };
            _controller.ModelState.AddModelError("Price", "Price is required");

            _mockProductRepo.Setup(repo => repo.GetAllStatusesAsync())
                .ReturnsAsync(new List<ProductStatus>());
            _mockProductRepo.Setup(repo => repo.GetAllCategoriesAsync())
                .ReturnsAsync(new List<Category>());

            // Act
            var result = await _controller.Edit(1, product, null);

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult.Model, Is.InstanceOf<Product>());
        }

        /// <summary>
        /// Test edit post với sản phẩm không thể chỉnh sửa redirect với lỗi
        /// </summary>
        [Test]
        public async Task Edit_Post_WhenProductNotEditable_RedirectsWithError()
        {
            // Arrange
            var product = new Product { ProductId = 1, Name = "Test" };

            // Mock product có inventory (không thể edit)
            var existingProduct = new Product
            {
                ProductId = 1,
                Inventory = new List<Inventory> { new Inventory() }
            };

            await _context.Products.AddAsync(existingProduct);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Edit(1, product, null);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult.ActionName, Is.EqualTo("Details"));
            Assert.That(_controller.TempData["Error"], Is.Not.Null);
        }

        /// <summary>
        /// Test edit post thành công với dữ liệu hợp lệ
        /// </summary>
        [Test]
        public async Task Edit_Post_WithValidData_UpdatesProduct()
        {
            // Arrange
            var existingProduct = new Product
            {
                ProductId = 1,
                Name = "Old Name",
                Price = 10000000m,
                SKU = "OLD123",
                StockCode = "STOCK001"
            };

            await _context.Products.AddAsync(existingProduct);
            await _context.SaveChangesAsync();

            var updatedProduct = new Product
            {
                ProductId = 1,
                Name = "New Name",
                Price = 15000000m,
                SKU = "OLD123", // Giữ nguyên SKU
                StockCode = "STOCK001" // Giữ nguyên StockCode
            };

            // Act
            var result = await _controller.Edit(1, updatedProduct, null);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult.ActionName, Is.EqualTo("Details"));

            // Kiểm tra dữ liệu đã được cập nhật
            var savedProduct = await _context.Products.FindAsync(1);
            Assert.That(savedProduct.Name, Is.EqualTo("New Name"));
            Assert.That(savedProduct.Price, Is.EqualTo(15000000m));
        }

        /// <summary>
        /// Test edit post với upload ảnh
        /// </summary>
        [Test]
        public async Task Edit_Post_WithImageUpload_Success()
        {
            // Arrange
            var existingProduct = new Product
            {
                ProductId = 1,
                Name = "Test Product",
                SKU = "TEST123"
            };

            await _context.Products.AddAsync(existingProduct);
            await _context.SaveChangesAsync();

            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.FileName).Returns("test.jpg");
            mockFile.Setup(f => f.ContentType).Returns("image/jpeg");

            var updatedProduct = new Product { ProductId = 1, Name = "Updated Product" };

            // Act
            var result = await _controller.Edit(1, updatedProduct, mockFile.Object);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        }

        // ===============================================
        // TEST ACTION IMAGE
        // ===============================================

        /// <summary>
        /// Test lấy ảnh sản phẩm với ảnh tồn tại
        /// </summary>
        [Test]
        public async Task Image_WithExistingImage_ReturnsFile()
        {
            // Arrange
            var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header
            var productImage = new ProductImage
            {
                ImageId = 1,
                ImagePath = imageBytes
            };

            await _context.ProductImages.AddAsync(productImage);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Image(1);

            // Assert
            Assert.That(result, Is.InstanceOf<FileContentResult>());
            var fileResult = result as FileContentResult;
            Assert.That(fileResult.FileContents, Is.EqualTo(imageBytes));
        }

        /// <summary>
        /// Test lấy ảnh sản phẩm với ảnh không tồn tại trả về NotFound
        /// </summary>
        [Test]
        public async Task Image_WithNonExistingImage_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Image(999);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        // ===============================================
        // TEST HELPER METHODS
        // ===============================================

        /// <summary>
        /// Test kiểm tra sản phẩm có thể chỉnh sửa
        /// </summary>
        [Test]
        public async Task CanEditProductAsync_WithNoInventory_ReturnsTrue()
        {
            // Arrange
            var product = new Product
            {
                ProductId = 1,
                Inventory = new List<Inventory>(),
                ExportDetails = new List<ExportReceiptDetail>(),
                ImportDetails = new List<ImportReceiptDetail>()
            };

            _mockProductRepo.Setup(repo => repo.GetByIdWithIncludesAsync(1))
                .ReturnsAsync(product);

            // Act - Sử dụng reflection để gọi private method
            var method = typeof(ProductsController).GetMethod("CanEditProductAsync",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var result = await (Task<bool>)method.Invoke(_controller, new object[] { 1 });

            // Assert
            Assert.That(result, Is.True);
        }

        /// <summary>
        /// Test kiểm tra sản phẩm không thể chỉnh sửa khi có inventory
        /// </summary>
        [Test]
        public async Task CanEditProductAsync_WithInventory_ReturnsFalse()
        {
            // Arrange
            var product = new Product
            {
                ProductId = 1,
                Inventory = new List<Inventory> { new Inventory() },
                ExportDetails = new List<ExportReceiptDetail>(),
                ImportDetails = new List<ImportReceiptDetail>()
            };

            _mockProductRepo.Setup(repo => repo.GetByIdWithIncludesAsync(1))
                .ReturnsAsync(product);

            // Act
            var method = typeof(ProductsController).GetMethod("CanEditProductAsync",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var result = await (Task<bool>)method.Invoke(_controller, new object[] { 1 });

            // Assert
            Assert.That(result, Is.False);
        }
    }
}