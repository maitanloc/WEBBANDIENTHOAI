// Tests/UnitTests/Repositories/ProductStatusRepositoryTests.cs
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Repository;

namespace WEBBANDIENTHOAI.Tests.UnitTests.Repositories
{
    [TestFixture]
    public class ProductStatusRepositoryTests
    {
        private AppDbContext _context;
        private ProductStatusRepository _repo;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ProductStatusRepoTestDb_" + Guid.NewGuid())
                .Options;
            _context = new AppDbContext(options);
            _repo = new ProductStatusRepository(_context);

            // Seed data cơ bản
            _context.ProductStatuses.AddRange(
                new ProductStatus { StatusId = 1, StatusName = "Available" },
                new ProductStatus { StatusId = 2, StatusName = "Out of Stock" }
            );
            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        /// <summary>
        /// Test lấy tất cả trạng thái sản phẩm kèm danh sách sản phẩm
        /// Kiểm tra xem repository có trả về đúng số lượng trạng thái không
        /// </summary>
        [Test]
        public async Task GetAllWithProductsAsync_ReturnsAllStatuses()
        {
            // Act
            var result = await _repo.GetAllWithProductsAsync();

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.All(ps => !string.IsNullOrEmpty(ps.StatusName)));
        }

        /// <summary>
        /// Test lấy trạng thái sản phẩm theo ID
        /// Kiểm tra xem repository có trả về đúng trạng thái theo ID không
        /// </summary>
        [TestCase(1, true)]  // ID tồn tại
        [TestCase(3, false)] // ID không tồn tại
        public async Task GetByIdAsync_ReturnsCorrectStatus(byte id, bool shouldExist)
        {
            // Act
            var result = await _repo.GetByIdAsync(id);

            // Assert
            if (shouldExist)
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.StatusId, Is.EqualTo(id));
            }
            else
            {
                Assert.That(result, Is.Null);
            }
        }

        /// <summary>
        /// Test lấy trạng thái sản phẩm theo ID kèm danh sách sản phẩm
        /// Kiểm tra xem repository có trả về trạng thái với navigation property Products không
        /// </summary>
        [Test]
        public async Task GetByIdWithProductsAsync_ReturnsStatusWithProducts()
        {
            // Arrange - Thêm sản phẩm để test relationship
            var product = new Product
            {
                ProductId = 1,
                Name = "Test Product",
                StatusId = 1,
                CategoryId = 1, // Cần CategoryId vì là FK
                SKU = "TEST001"
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.GetByIdWithProductsAsync(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusId, Is.EqualTo(1));
            Assert.That(result.Products, Is.Not.Null);
            Assert.That(result.Products.Any(), Is.True);
        }

        /// <summary>
        /// Test kiểm tra trạng thái sản phẩm có tồn tại không
        /// Kiểm tra xem phương thức ExistsAsync trả về đúng kết quả
        /// </summary>
        [TestCase(1, true)]   // Tồn tại
        [TestCase(99, false)] // Không tồn tại
        public async Task ExistsAsync_ReturnsCorrect(byte id, bool expected)
        {
            // Act
            var result = await _repo.ExistsAsync(id);

            // Assert
            Assert.That(result, Is.EqualTo(expected));
        }

        /// <summary>
        /// Test kiểm tra trạng thái sản phẩm có sản phẩm nào không
        /// Kiểm tra xem phương thức HasProductsAsync trả về đúng trạng thái
        /// </summary>
        [Test]
        public async Task HasProductsAsync_ReturnsTrue_WhenStatusHasProducts()
        {
            // Arrange - Thêm sản phẩm cho status 1
            var product = new Product
            {
                ProductId = 1,
                Name = "Test Product",
                StatusId = 1,
                CategoryId = 1,
                SKU = "TEST001"
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.HasProductsAsync(1);

            // Assert
            Assert.That(result, Is.True);
        }

        /// <summary>
        /// Test kiểm tra trạng thái sản phẩm không có sản phẩm nào
        /// Kiểm tra xem phương thức HasProductsAsync trả về false khi không có sản phẩm
        /// </summary>
        [Test]
        public async Task HasProductsAsync_ReturnsFalse_WhenStatusHasNoProducts()
        {
            // Act - Status 2 không có sản phẩm nào
            var result = await _repo.HasProductsAsync(2);

            // Assert
            Assert.That(result, Is.False);
        }

        /// <summary>
        /// Test tạo mới trạng thái sản phẩm thành công
        /// Kiểm tra xem trạng thái mới có được thêm vào database không
        /// </summary>
        [Test]
        public async Task CreateAsync_AddsNewStatus()
        {
            // Arrange
            var newStatus = new ProductStatus { StatusId = 3, StatusName = "Discontinued" };

            // Act
            var result = await _repo.CreateAsync(newStatus);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(await _context.ProductStatuses.CountAsync(), Is.EqualTo(3));

            var savedStatus = await _context.ProductStatuses.FindAsync((byte)3);
            Assert.That(savedStatus, Is.Not.Null);
            Assert.That(savedStatus.StatusName, Is.EqualTo("Discontinued"));
        }

        /// <summary>
        /// Test tạo mới trạng thái sản phẩm thất bại khi trùng ID
        /// Kiểm tra xem repository có xử lý lỗi và trả về false không
        /// </summary>
        [Test]
        public async Task CreateAsync_ReturnsFalse_WhenDuplicateId()
        {
            // Arrange - Tạo status với ID đã tồn tại
            var duplicateStatus = new ProductStatus { StatusId = 1, StatusName = "Duplicate" };

            // Act
            var result = await _repo.CreateAsync(duplicateStatus);

            // Assert
            Assert.That(result, Is.False);
        }

        /// <summary>
        /// Test cập nhật trạng thái sản phẩm thành công
        /// Kiểm tra xem thông tin trạng thái có được cập nhật đúng không
        /// </summary>
        [Test]
        public async Task UpdateAsync_UpdatesStatusSuccessfully()
        {
            // Arrange
            var status = await _context.ProductStatuses.FindAsync((byte)1);
            status.StatusName = "Available - Updated";

            // Act
            var result = await _repo.UpdateAsync(status);

            // Assert
            Assert.That(result, Is.True);

            var updatedStatus = await _context.ProductStatuses.FindAsync((byte)1);
            Assert.That(updatedStatus.StatusName, Is.EqualTo("Available - Updated"));
        }

        /// <summary>
        /// Test cập nhật trạng thái sản phẩm không tồn tại
        /// Kiểm tra xem repository có trả về false khi cập nhật status không tồn tại không
        /// </summary>
        [Test]
        public async Task UpdateAsync_ReturnsFalse_WhenStatusNotExists()
        {
            // Arrange
            var nonExistentStatus = new ProductStatus { StatusId = 99, StatusName = "Non-existent" };

            // Act
            var result = await _repo.UpdateAsync(nonExistentStatus);

            // Assert
            Assert.That(result, Is.False);
        }

        /// <summary>
        /// Test xóa trạng thái sản phẩm thành công khi không có sản phẩm
        /// Kiểm tra xem trạng thái có được xóa khỏi database không
        /// </summary>
        [Test]
        public async Task DeleteAsync_RemovesStatus_WhenNoProducts()
        {
            // Act
            var result = await _repo.DeleteAsync(2); // Status 2 không có products

            // Assert
            Assert.That(result, Is.True);
            Assert.That(await _context.ProductStatuses.CountAsync(), Is.EqualTo(1));

            var deletedStatus = await _context.ProductStatuses.FindAsync((byte)2);
            Assert.That(deletedStatus, Is.Null);
        }

        /// <summary>
        /// Test xóa trạng thái sản phẩm có sản phẩm
        /// Kiểm tra xem repository có xử lý lỗi FK constraint và trả về false không
        /// </summary>
        [Test]
        public async Task DeleteAsync_ReturnsFalse_WhenStatusHasProducts()
        {
            // Arrange - Thêm sản phẩm cho status 1
            var product = new Product
            {
                ProductId = 1,
                Name = "Test Product",
                StatusId = 1,
                CategoryId = 1,
                SKU = "TEST001"
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.DeleteAsync(1);

            // Assert
            Assert.That(result, Is.False);
            Assert.That(await _context.ProductStatuses.CountAsync(), Is.EqualTo(2)); // Vẫn còn 2 status
        }

        /// <summary>
        /// Test xóa trạng thái sản phẩm không tồn tại
        /// Kiểm tra xem repository có trả về false khi xóa status không tồn tại không
        /// </summary>
        [Test]
        public async Task DeleteAsync_ReturnsFalse_WhenStatusNotExists()
        {
            // Act
            var result = await _repo.DeleteAsync(99);

            // Assert
            Assert.That(result, Is.False);
        }

        /// <summary>
        /// Test toàn diện: Tạo -> Cập nhật -> Xóa trạng thái sản phẩm
        /// Kiểm tra toàn bộ flow CRUD của repository
        /// </summary>
        [Test]
        public async Task FullCRUD_Flow_Successful()
        {
            // CREATE
            var newStatus = new ProductStatus { StatusId = 10, StatusName = "New Status" };
            var createResult = await _repo.CreateAsync(newStatus);
            Assert.That(createResult, Is.True);
            Assert.That(await _context.ProductStatuses.CountAsync(), Is.EqualTo(3));

            // READ
            var retrievedStatus = await _repo.GetByIdAsync(10);
            Assert.That(retrievedStatus, Is.Not.Null);
            Assert.That(retrievedStatus.StatusName, Is.EqualTo("New Status"));

            // UPDATE
            retrievedStatus.StatusName = "Updated Status";
            var updateResult = await _repo.UpdateAsync(retrievedStatus);
            Assert.That(updateResult, Is.True);

            var updatedStatus = await _repo.GetByIdAsync(10);
            Assert.That(updatedStatus.StatusName, Is.EqualTo("Updated Status"));

            // DELETE
            var deleteResult = await _repo.DeleteAsync(10);
            Assert.That(deleteResult, Is.True);
            Assert.That(await _context.ProductStatuses.CountAsync(), Is.EqualTo(2));

            // VERIFY DELETED
            var deletedStatus = await _repo.GetByIdAsync(10);
            Assert.That(deletedStatus, Is.Null);
        }
    }
}