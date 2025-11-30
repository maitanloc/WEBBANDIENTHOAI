// Tests/UnitTests/Controllers/ProductStatusesControllerTests.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using WEBBANDIENTHOAI.Controllers.Admin;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Repository.Admin;

namespace WEBBANDIENTHOAI.Tests.UnitTests.Controllers
{
    [TestFixture]
    public class ProductStatusesControllerTests
    {
        private Mock<IProductStatusRepository> _mockRepo;
        private ProductStatusesController _controller;

        [SetUp]
        public void Setup()
        {
            _mockRepo = new Mock<IProductStatusRepository>();
            _controller = new ProductStatusesController(_mockRepo.Object);

            // Cấu hình TempData cho controller
            _controller.TempData = new TempDataDictionary(
                new Microsoft.AspNetCore.Http.DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());
        }

        // ===============================================
        // TEST ACTION PRODUCTSTATUSES (GET)
        // ===============================================

        /// <summary>
        /// Test trang danh sách trạng thái sản phẩm trả về view với dữ liệu
        /// </summary>
        [Test]
        public async Task ProductStatuses_ReturnsViewWithStatuses()
        {
            // Arrange
            var mockStatuses = new List<ProductStatus>
            {
                new ProductStatus { StatusId = 1, StatusName = "InStock", Description = "Còn hàng" },
                new ProductStatus { StatusId = 2, StatusName = "OutOfStock", Description = "Hết hàng" }
            };

            _mockRepo.Setup(repo => repo.GetAllWithProductsAsync())
                .ReturnsAsync(mockStatuses);

            // Act
            var result = await _controller.ProductStatuses();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult.Model, Is.InstanceOf<List<ProductStatus>>());
            Assert.That(viewResult.ViewName, Is.EqualTo("~/Views/Products/ProductStatuses.cshtml"));
        }

        // ===============================================
        // TEST ACTION CREATESTATUS (POST)
        // ===============================================

        /// <summary>
        /// Test tạo trạng thái mới thành công với dữ liệu hợp lệ
        /// </summary>
        [Test]
        public async Task CreateStatus_WithValidData_ReturnsRedirectWithSuccess()
        {
            // Arrange
            var dto = new CreateProductStatusDto
            {
                StatusName = "NewStatus",
                Description = "Mô tả trạng thái mới"
            };

            _mockRepo.Setup(repo => repo.CreateAsync(It.IsAny<ProductStatus>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.CreateStatus(dto);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult.ActionName, Is.EqualTo("ProductStatuses"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("ProductStatuses"));
            Assert.That(_controller.TempData["Success"], Is.EqualTo("Thêm trạng thái thành công!"));
        }

        /// <summary>
        /// Test tạo trạng thái thất bại khi repository trả về false
        /// </summary>
        [Test]
        public async Task CreateStatus_WhenRepositoryFails_ReturnsRedirectWithError()
        {
            // Arrange
            var dto = new CreateProductStatusDto
            {
                StatusName = "NewStatus",
                Description = "Mô tả trạng thái mới"
            };

            _mockRepo.Setup(repo => repo.CreateAsync(It.IsAny<ProductStatus>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.CreateStatus(dto);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(_controller.TempData["Error"], Is.EqualTo("Lỗi khi thêm trạng thái. Vui lòng thử lại."));
        }

        /// <summary>
        /// Test tạo trạng thái thất bại khi ModelState không hợp lệ
        /// </summary>
        [Test]
        public async Task CreateStatus_WithInvalidModel_ReturnsRedirectWithError()
        {
            // Arrange
            var dto = new CreateProductStatusDto
            {
                StatusName = "", // Invalid - empty
                Description = "Mô tả"
            };

            _controller.ModelState.AddModelError("StatusName", "StatusName là bắt buộc");

            // Act
            var result = await _controller.CreateStatus(dto);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(_controller.TempData["Error"], Is.EqualTo("Dữ liệu không hợp lệ!"));
        }

        // ===============================================
        // TEST ACTION EDITSTATUS (POST)
        // ===============================================

        /// <summary>
        /// Test cập nhật trạng thái thành công với dữ liệu hợp lệ
        /// </summary>
        [Test]
        public async Task EditStatus_WithValidData_ReturnsRedirectWithSuccess()
        {
            // Arrange
            var existingStatus = new ProductStatus
            {
                StatusId = 1,
                StatusName = "OldName",
                Description = "Old Description"
            };

            var dto = new UpdateProductStatusDto
            {
                StatusId = 1,
                StatusName = "NewName",
                Description = "New Description"
            };

            _mockRepo.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(existingStatus);
            _mockRepo.Setup(repo => repo.UpdateAsync(It.IsAny<ProductStatus>()))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.EditStatus(dto);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(_controller.TempData["Success"], Is.EqualTo("Cập nhật trạng thái thành công!"));
        }

        /// <summary>
        /// Test cập nhật trạng thái thất bại khi không tìm thấy trạng thái
        /// </summary>
        [Test]
        public async Task EditStatus_WhenStatusNotFound_ReturnsRedirectWithError()
        {
            // Arrange
            var dto = new UpdateProductStatusDto
            {
                StatusId = 255, // Non-existent ID
                StatusName = "NewName",
                Description = "New Description"
            };

            _mockRepo.Setup(repo => repo.GetByIdAsync(255))
                .ReturnsAsync((ProductStatus)null);

            // Act
            var result = await _controller.EditStatus(dto);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult.ActionName, Is.EqualTo("ProductStatuses"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("ProductStatuses"));
            Assert.That(_controller.TempData["Error"], Is.EqualTo("Không tìm thấy trạng thái!"));
        }

        /// <summary>
        /// Test cập nhật trạng thái thất bại khi repository update trả về false
        /// </summary>
        [Test]
        public async Task EditStatus_WhenUpdateFails_ReturnsRedirectWithError()
        {
            // Arrange
            var existingStatus = new ProductStatus
            {
                StatusId = 1,
                StatusName = "OldName",
                Description = "Old Description"
            };

            var dto = new UpdateProductStatusDto
            {
                StatusId = 1,
                StatusName = "NewName",
                Description = "New Description"
            };

            _mockRepo.Setup(repo => repo.GetByIdAsync(1))
                .ReturnsAsync(existingStatus);
            _mockRepo.Setup(repo => repo.UpdateAsync(It.IsAny<ProductStatus>()))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.EditStatus(dto);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(_controller.TempData["Error"], Is.EqualTo("Lỗi khi cập nhật trạng thái!"));
        }

        /// <summary>
        /// Test cập nhật trạng thái thất bại khi ModelState không hợp lệ
        /// </summary>
        [Test]
        public async Task EditStatus_WithInvalidModel_ReturnsRedirectWithError()
        {
            // Arrange
            var dto = new UpdateProductStatusDto
            {
                StatusId = 1,
                StatusName = "", // Invalid - empty
                Description = "New Description"
            };

            _controller.ModelState.AddModelError("StatusName", "StatusName là bắt buộc");

            // Act
            var result = await _controller.EditStatus(dto);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(_controller.TempData["Error"], Is.EqualTo("Dữ liệu không hợp lệ!"));
        }

        // ===============================================
        // TEST ACTION DELETESTATUS (POST)
        // ===============================================

        /// <summary>
        /// Test xóa trạng thái thành công khi không có sản phẩm nào sử dụng
        /// </summary>
        [Test]
        public async Task DeleteStatus_WithNoProducts_ReturnsRedirectWithSuccess()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.HasProductsAsync(1))
                .ReturnsAsync(false);
            _mockRepo.Setup(repo => repo.DeleteAsync(1))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteStatus(1);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(_controller.TempData["Success"], Is.EqualTo("Xóa trạng thái thành công!"));
        }

        /// <summary>
        /// Test xóa trạng thái thất bại khi có sản phẩm đang sử dụng
        /// </summary>
        [Test]
        public async Task DeleteStatus_WithExistingProducts_ReturnsRedirectWithError()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.HasProductsAsync(1))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteStatus(1);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(_controller.TempData["Error"], Is.EqualTo("Không thể xóa trạng thái này vì đang có sản phẩm sử dụng!"));
        }

        /// <summary>
        /// Test xóa trạng thái thất bại khi repository delete trả về false
        /// </summary>
        [Test]
        public async Task DeleteStatus_WhenDeleteFails_ReturnsRedirectWithError()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.HasProductsAsync(1))
                .ReturnsAsync(false);
            _mockRepo.Setup(repo => repo.DeleteAsync(1))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteStatus(1);

            // Assert
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            Assert.That(_controller.TempData["Error"], Is.EqualTo("Lỗi khi xóa trạng thái!"));
        }
    }

    

 
}