// Tests/UnitTests/Controllers/ProductsControllerTests.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using WebBanDienThoai.Models;
using WEBBANDIENTHOAI.Controllers;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Repository;
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
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
        }

        [TearDown]
        public void TearDown()
        {
            // Xóa database sau mỗi test để đảm bảo test độc lập
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // ===============================================
        // TEST ACTION INDEX
        // ===============================================

        /// <summary>
        /// Test trang danh sách sản phẩm khi áp dụng bộ lọc
        /// Kiểm tra xem controller có trả về view với sản phẩm đã lọc đúng không
        /// </summary>
        [Test]
        public async Task Index_ReturnsViewWithFilteredProducts_WhenFiltersApplied()
        {
            // Arrange - Chuẩn bị dữ liệu test
            var mockProducts = new List<ProductListItemVm> {
                new ProductListItemVm { ProductId = 1, Name = "iPhone 15" }
            };

            _mockProductRepo.Setup(r => r.GetFilteredAsync("iPhone", 1, 1, 10000000m, 30000000m, true, "price_desc", 1, 20))
                .ReturnsAsync(() => (mockProducts, 1));

            // Act - Gọi action Index với các tham số lọc
            var result = await _controller.Index("iPhone", 1, 1, 10000000m, 30000000m, true, "price_desc", 1, 20);

            // Assert - Kiểm tra kết quả
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            var model = viewResult.Model as IEnumerable<ProductListItemVm>;
            Assert.That(model.Count(), Is.EqualTo(1));
            Assert.That(viewResult.ViewData["TotalCount"], Is.EqualTo(1));
        }

        /// <summary>
        /// Test trang danh sách sản phẩm khi request là AJAX
        /// Kiểm tra xem có trả về PartialView thay vì View đầy đủ không
        /// </summary>
        [Test]
        public async Task Index_ReturnsPartialView_WhenAjaxRequest()
        {
            // Arrange - Giả lập request AJAX
            var mockProducts = new List<ProductListItemVm> {
                new ProductListItemVm { ProductId = 1 }
            };

            _mockProductRepo.Setup(r => r.GetFilteredAsync(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<byte?>(),
                It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<bool?>(),
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(() => (mockProducts, 1));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    Request = { Headers = { ["X-Requested-With"] = "XMLHttpRequest" } }
                }
            };

            // Act
            var result = await _controller.Index(null, null, null, null, null, null, null, 1, 20);

            // Assert - Kiểm tra trả về PartialView
            Assert.That(result, Is.InstanceOf<PartialViewResult>());
            var partialResult = result as PartialViewResult;
            Assert.That(partialResult.ViewName, Is.EqualTo("_ProductsIndexPartial"));
        }

        /// <summary>
        /// Test xử lý lỗi khi có exception xảy ra trong action Index
        /// Kiểm tra xem controller có xử lý lỗi đúng cách không
        /// </summary>
        [Test]
        public async Task Index_HandlesException_ReturnsEmptyViewWithError()
        {
            // Arrange - Giả lập repository throw exception
            _mockProductRepo.Setup(r => r.GetFilteredAsync(
                It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<byte?>(),
                It.IsAny<decimal?>(), It.IsAny<decimal?>(), It.IsAny<bool?>(),
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .ThrowsAsync(new Exception("Test error"));

            // Act
            var result = await _controller.Index(null, null, null, null, null, null, null, 1, 20);

            // Assert - Kiểm tra trả về view rỗng và có thông báo lỗi
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
        /// Test chi tiết sản phẩm khi ID là null
        /// Kiểm tra xem có trả về NotFound không
        /// </summary>
        [Test]
        public async Task Details_ReturnsNotFound_WhenIdIsNull()
        {
            // Act
            var result = await _controller.Details(null);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        /// <summary>
        /// Test chi tiết sản phẩm khi sản phẩm không tồn tại
        /// Kiểm tra xem có trả về NotFound không
        /// </summary>
        [Test]
        public async Task Details_ReturnsNotFound_WhenProductNotExist()
        {
            // Act
            var result = await _controller.Details(999);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        /// <summary>
        /// Test chi tiết sản phẩm khi ID hợp lệ nhưng không thể chỉnh sửa
        /// Kiểm tra xem có trả về view với thông tin sản phẩm và cờ CanEdit = false không
        /// </summary>
        [Test]
        public async Task Details_ReturnsViewWithProduct_WhenValidAndCanEditFalse()
        {
            // Arrange - Tạo dữ liệu sản phẩm test
            var category = new Category
            {
                CategoryId = 1,
                CategoryName = "Smartphones",
                Description = "Mô tả giả"
            };
            var status = new ProductStatus
            {
                StatusId = 1,
                StatusName = "InStock"
            };
            var product = new Product
            {
                ProductId = 1,
                CategoryId = 1,
                StatusId = 1,
                Name = "iPhone 15",
                SKU = "IP15"
            };

            await _context.Categories.AddAsync(category);
            await _context.ProductStatuses.AddAsync(status);
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Details(1);

            // Assert - Kiểm tra trả về view với sản phẩm và không cho phép edit
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            var model = viewResult.Model as Product;
            Assert.That(model.ProductId, Is.EqualTo(1));
            Assert.That(viewResult.ViewData["CanEdit"], Is.False);
        }

        /// <summary>
        /// Test xử lý exception trong action Details
        /// Kiểm tra xem có chuyển hướng về trang Index với thông báo lỗi không
        /// </summary>
        [Test]
        public async Task Details_HandlesException_RedirectsWithError()
        {
            // Arrange - Tạo mock context để giả lập lỗi database
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ExceptionTestDb")
                .Options;
            var mockContext = new Mock<AppDbContext>(options);

            var mockProducts = new Mock<DbSet<Product>>();
            mockProducts.Setup(m => m.FindAsync(It.IsAny<object[]>())).Throws(new Exception("Test error"));
            mockContext.Setup(c => c.Products).Returns(mockProducts.Object);

            var controller = new ProductsController(_mockProductRepo.Object, _mockLogger.Object, mockContext.Object);
            controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            // Act
            var result = await controller.Details(1);

            // Assert - Kiểm tra chuyển hướng và có thông báo lỗi
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(controller.TempData["Error"], Is.Not.Null);
        }

        // ===============================================
        // TEST ACTION EDIT (GET)
        // ===============================================

        /// <summary>
        /// Test trang chỉnh sửa sản phẩm khi ID là null
        /// Kiểm tra xem có trả về NotFound không
        /// </summary>
        [Test]
        public async Task Edit_Get_ReturnsNotFound_WhenIdNull()
        {
            // Act
            var result = await _controller.Edit(null);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        /// <summary>
        /// Test trang chỉnh sửa khi sản phẩm không thể chỉnh sửa
        /// Kiểm tra xem có chuyển hướng về trang Details với thông báo lỗi không
        /// </summary>
        [Test]
        public async Task Edit_Get_RedirectsWithError_WhenNotEditable()
        {
            // Arrange - Tạo sản phẩm không có inventory (không thể edit)
            var product = new Product
            {
                ProductId = 1,
                Name = "iPhone 15"
            };
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Edit(1);

            // Assert - Kiểm tra chuyển hướng và có thông báo lỗi
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirect = result as RedirectToActionResult;
            Assert.That(redirect.ActionName, Is.EqualTo("Details"));
            Assert.That(_controller.TempData["Error"], Is.Not.Null);
        }

        /// <summary>
        /// Test trang chỉnh sửa khi sản phẩm có thể chỉnh sửa
        /// Kiểm tra xem có trả về view với dữ liệu sản phẩm và dropdown lists không
        /// </summary>
        [Test]
        public async Task Edit_Get_ReturnsViewWithProduct_WhenEditable()
        {
            // Arrange - Tạo sản phẩm có inventory (có thể edit)
            var product = new Product
            {
                ProductId = 1,
                Name = "iPhone 15"
            };
            var inventory = new Inventory
            {
                InventoryId = 1,
                ProductId = 1
            };
            await _context.Products.AddAsync(product);
            await _context.Inventory.AddAsync(inventory);
            await _context.SaveChangesAsync();

            // Mock dữ liệu cho dropdown lists
            _mockProductRepo.Setup(r => r.GetAllStatusesAsync()).ReturnsAsync(new List<ProductStatus>());
            _mockProductRepo.Setup(r => r.GetAllCategoriesAsync()).ReturnsAsync(new List<Category>());

            // Act
            var result = await _controller.Edit(1);

            // Assert - Kiểm tra trả về view với đầy đủ dữ liệu
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            var model = viewResult.Model as Product;
            Assert.That(model.ProductId, Is.EqualTo(1));
            Assert.That(viewResult.ViewData["StatusId"], Is.InstanceOf<SelectList>());
            Assert.That(viewResult.ViewData["CategoryId"], Is.InstanceOf<SelectList>());
            Assert.That(viewResult.ViewData["CanChangeKeys"], Is.True);
        }

        // ===============================================
        // TEST ACTION EDIT (POST)
        // ===============================================

        /// <summary>
        /// Test cập nhật sản phẩm khi ID không khớp
        /// Kiểm tra xem có trả về NotFound không
        /// </summary>
        [Test]
        public async Task Edit_Post_ReturnsNotFound_WhenIdMismatch()
        {
            // Arrange - ID trong route (1) không khớp với ID trong model (2)
            var product = new Product
            {
                ProductId = 2
            };

            // Act
            var result = await _controller.Edit(1, product, null);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        /// <summary>
        /// Test cập nhật sản phẩm khi sản phẩm không thể chỉnh sửa
        /// Kiểm tra xem có chuyển hướng với thông báo lỗi không
        /// </summary>
        [Test]
        public async Task Edit_Post_RedirectsWithError_WhenNotEditable()
        {
            // Arrange - Sản phẩm không có inventory
            var product = new Product
            {
                ProductId = 1
            };

            // Act
            var result = await _controller.Edit(1, product, null);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(_controller.TempData["Error"], Is.Not.Null);
        }

        /// <summary>
        /// Test cập nhật sản phẩm thành công khi dữ liệu hợp lệ và không thay đổi khóa
        /// Kiểm tra xem sản phẩm có được cập nhật đúng không
        /// </summary>
        [Test]
        public async Task Edit_Post_UpdatesProduct_WhenValidNoKeyChange()
        {
            // Arrange - Tạo sản phẩm tồn tại trong database
            var existing = new Product
            {
                ProductId = 1,
                Name = "Old",
                SKU = "IP15",
                Price = 25000000m,
                StatusId = 1,
                CategoryId = 1,
                StockCode = "STOCK001"
            };
            var inventory = new Inventory
            {
                InventoryId = 1,
                ProductId = 1,
                CurrentQuantity = 10
            };
            await _context.Products.AddAsync(existing);
            await _context.Inventory.AddAsync(inventory);
            await _context.SaveChangesAsync();

            // Dữ liệu cập nhật - không thay đổi SKU và StockCode
            var updated = new Product
            {
                ProductId = 1,
                Name = "New Name",
                SKU = "IP15",
                Price = 30000000m,
                StatusId = 2,
                CategoryId = 2,
                StockCode = "STOCK001"
            };

            // Act
            var result = await _controller.Edit(1, updated, null);

            // Assert - Kiểm tra cập nhật thành công và chuyển hướng
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var saved = await _context.Products.FindAsync(1);
            Assert.That(saved.Name, Is.EqualTo("New Name"));
            Assert.That(saved.Price, Is.EqualTo(30000000m));
            Assert.That(saved.StatusId, Is.EqualTo(2));
            Assert.That(saved.SKU, Is.EqualTo("IP15")); // Không thay đổi
            Assert.That(_controller.TempData["Success"], Is.Not.Null);
        }

        /// <summary>
        /// Test ngăn chặn thay đổi khóa (SKU/StockCode) khi có lịch sử
        /// Kiểm tra xem có thông báo lỗi khi cố gắng thay đổi khóa không
        /// </summary>
        [Test]
        public async Task Edit_Post_PreventsKeyChange_WhenHasHistory()
        {
            // Arrange - Sản phẩm có inventory (được coi là có lịch sử)
            var existing = new Product
            {
                ProductId = 1,
                SKU = "OldSKU",
                StockCode = "OldCode",
                Name = "Test Product"
            };
            var inventory = new Inventory
            {
                InventoryId = 1,
                ProductId = 1
            };
            await _context.Products.AddAsync(existing);
            await _context.Inventory.AddAsync(inventory);
            await _context.SaveChangesAsync();

            // Dữ liệu cập nhật - cố gắng thay đổi SKU và StockCode
            var updated = new Product
            {
                ProductId = 1,
                SKU = "NewSKU",
                StockCode = "NewCode",
                Name = "Test Product"
            };

            // Act
            var result = await _controller.Edit(1, updated, null);

            // Assert - Kiểm tra không cho phép thay đổi và có thông báo lỗi
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_controller.TempData["Error"], Is.Not.Null);
        }

        /// <summary>
        /// Test upload ảnh sản phẩm thành công
        /// Kiểm tra xem ảnh có được xử lý đúng không
        /// </summary>
        [Test]
        public async Task Edit_Post_HandlesImageUpload_Success()
        {
            // Arrange - Tạo sản phẩm và mock file ảnh
            var existing = new Product
            {
                ProductId = 1,
                Name = "iPhone",
                SKU = "IP15"
            };
            var inventory = new Inventory
            {
                InventoryId = 1,
                ProductId = 1,
                CurrentQuantity = 10
            };
            await _context.Products.AddAsync(existing);
            await _context.Inventory.AddAsync(inventory);
            await _context.SaveChangesAsync();

            // Mock file ảnh
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(1024);
            mockFile.Setup(f => f.FileName).Returns("test.jpg");
            mockFile.Setup(f => f.ContentType).Returns("image/jpeg");
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Edit(1, existing, mockFile.Object);

            // Assert - Kiểm tra cập nhật thành công
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        }

        /// <summary>
        /// Test xử lý exception khi cập nhật sản phẩm
        /// Kiểm tra xem có trả về view với thông báo lỗi không
        /// </summary>
        [Test]
        public async Task Edit_Post_HandlesException_ReturnsViewWithError()
        {
            // Arrange - Tạo model state không hợp lệ
            var product = new Product
            {
                ProductId = 1,
                Name = "Test"
            };
            _controller.ModelState.AddModelError("Name", "Error");

            // Mock dữ liệu cho dropdown lists
            _mockProductRepo.Setup(r => r.GetAllStatusesAsync()).ReturnsAsync(new List<ProductStatus>());
            _mockProductRepo.Setup(r => r.GetAllCategoriesAsync()).ReturnsAsync(new List<Category>());

            // Act
            var result = await _controller.Edit(1, product, null);

            // Assert - Kiểm tra trả về view với thông tin lỗi
            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_controller.ViewData["StatusId"], Is.InstanceOf<SelectList>());
        }

        // ===============================================
        // TEST ACTION IMAGE
        // ===============================================

        /// <summary>
        /// Test lấy ảnh sản phẩm khi ảnh tồn tại
        /// Kiểm tra xem có trả về file ảnh đúng không
        /// </summary>
        [Test]
        public async Task Image_ReturnsFile_WhenImageExists()
        {
            // Arrange - Mock dữ liệu ảnh
            _mockProductRepo.Setup(r => r.GetImageBytesAsync(1)).ReturnsAsync(new byte[] { 0x89, 0x50 });

            // Act
            var result = await _controller.Image(1);

            // Assert - Kiểm tra trả về file ảnh
            Assert.That(result, Is.InstanceOf<FileContentResult>());
            var fileResult = result as FileContentResult;
            Assert.That(fileResult.ContentType, Is.EqualTo("image/jpeg"));
        }

        /// <summary>
        /// Test lấy ảnh sản phẩm khi không có ảnh
        /// Kiểm tra xem có trả về NotFound không
        /// </summary>
        [Test]
        public async Task Image_ReturnsNotFound_WhenNoImage()
        {
            // Arrange - Mock không có ảnh
            _mockProductRepo.Setup(r => r.GetImageBytesAsync(999)).ReturnsAsync((byte[])null);

            // Act
            var result = await _controller.Image(999);

            // Assert
            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
}