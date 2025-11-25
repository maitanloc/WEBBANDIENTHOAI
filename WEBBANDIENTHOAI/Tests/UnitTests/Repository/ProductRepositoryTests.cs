// Tests/UnitTests/Repositories/ProductRepositoryTests.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using WebBanDienThoai.Models;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Repository;
using WEBBANDIENTHOAI.ViewModels;

namespace WEBBANDIENTHOAI.Tests.UnitTests.Repositories
{
    [TestFixture]
    public class ProductRepositoryTests
    {
        private AppDbContext _context;
        private ProductRepository _repo;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "ProductRepoTestDb_" + Guid.NewGuid())
                .Options;
            _context = new AppDbContext(options);
            _repo = new ProductRepository(_context);

            // Seed data đầy đủ để tránh lỗi FK
            var cat = new Category { CategoryId = 1, CategoryName = "Phone" };
            var status = new ProductStatus { StatusId = 1, StatusName = "InStock" };
            _context.Categories.Add(cat);
            _context.ProductStatuses.Add(status);
            _context.SaveChanges();

            _context.Products.AddRange(
                new Product { ProductId = 1, Name = "iPhone 15", Price = 25000000m, SKU = "IP15", CategoryId = 1, StatusId = 1, CreatedAt = DateTime.UtcNow },
                new Product { ProductId = 2, Name = "Samsung S24", Price = 22000000m, SKU = "SS24", CategoryId = 1, StatusId = 1, CreatedAt = DateTime.UtcNow }
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
        /// Test lấy danh sách sản phẩm với cờ chỉnh sửa
        /// Kiểm tra xem repository có trả về đúng số lượng sản phẩm và cờ IsEditable không
        /// </summary>
        [Test]
        public async Task GetAllForListWithEditableFlagAsync_ReturnsProductsWithFlags()
        {
            // Act
            var result = await _repo.GetAllForListWithEditableFlagAsync(10);

            // Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.All(p => p.IsEditable == false));
            // Mặc định là false vì chưa có inventory, import, export
        }

        /// <summary>
        /// Test lấy sản phẩm theo ID với đầy đủ thông tin liên quan
        /// Kiểm tra xem repository có trả về sản phẩm đúng ID và bao gồm các navigation properties không
        /// </summary>
        [TestCase(1, true)]  // ID tồn tại
        [TestCase(999, false)] // ID không tồn tại
        public async Task GetByIdWithIncludesAsync_ReturnsProduct_WhenIdExists(int id, bool expectedExists)
        {
            // Act
            var result = await _repo.GetByIdWithIncludesAsync(id);

            // Assert
            Assert.That(result != null, Is.EqualTo(expectedExists));
            if (expectedExists)
            {
                Assert.That(result.ProductId, Is.EqualTo(id));
                // Có thể thêm kiểm tra các navigation properties nếu cần
            }
        }

        /// <summary>
        /// Test lấy thông tin sản phẩm dạng list item theo ID
        /// Kiểm tra xem repository có trả về đúng thông tin cơ bản của sản phẩm không
        /// </summary>
        [Test]
        public async Task GetListItemByIdAsync_ReturnsProductListItem_WhenIdExists()
        {
            // Act
            var result = await _repo.GetListItemByIdAsync(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ProductId, Is.EqualTo(1));
            Assert.That(result.Name, Is.EqualTo("iPhone 15"));
            Assert.That(result.IsEditable, Is.False);
        }

        /// <summary>
        /// Test lấy thông tin sản phẩm dạng list item khi ID không tồn tại
        /// Kiểm tra xem repository có trả về null không
        /// </summary>
        [Test]
        public async Task GetListItemByIdAsync_ReturnsNull_WhenIdNotExists()
        {
            // Act
            var result = await _repo.GetListItemByIdAsync(999);

            // Assert
            Assert.That(result, Is.Null);
        }

        /// <summary>
        /// Test tìm kiếm sản phẩm theo từ khóa
        /// Kiểm tra xem bộ lọc tìm kiếm có hoạt động đúng không
        /// </summary>
        [Test]
        public async Task GetFilteredAsync_WithSearch_ReturnsFilteredProducts()
        {
            // Act
            var (items, total) = await _repo.GetFilteredAsync("iPhone", null, null, null, null, null, null, 1, 20);

            // Assert
            Assert.That(items.Count(), Is.EqualTo(1));
            Assert.That(total, Is.EqualTo(1));
            Assert.That(items.First().Name, Is.EqualTo("iPhone 15"));
        }

        /// <summary>
        /// Test lọc sản phẩm theo khoảng giá
        /// Kiểm tra xem bộ lọc giá có trả về đúng số lượng sản phẩm trong khoảng giá không
        /// </summary>
        [TestCase(20000000, 30000000, 2)]  // Cả 2 sản phẩm trong khoảng giá
        [TestCase(24000000, 26000000, 1)]  // Chỉ iPhone 15 trong khoảng
        [TestCase(30000000, 40000000, 0)]  // Không có sản phẩm nào
        public async Task GetFilteredAsync_WithPriceRange_ReturnsCorrectCount(decimal min, decimal max, int expected)
        {
            // Act
            var (items, total) = await _repo.GetFilteredAsync(null, null, null, min, max, null, null, 1, 20);

            // Assert
            Assert.That(total, Is.EqualTo(expected));
        }

        /// <summary>
        /// Test lọc sản phẩm theo danh mục
        /// Kiểm tra xem bộ lọc danh mục có hoạt động đúng không
        /// </summary>
        [Test]
        public async Task GetFilteredAsync_WithCategoryFilter_ReturnsCategoryProducts()
        {
            // Act
            var (items, total) = await _repo.GetFilteredAsync(null, 1, null, null, null, null, null, 1, 20);

            // Assert
            Assert.That(total, Is.EqualTo(2)); // Cả 2 sản phẩm đều thuộc category 1
        }

        /// <summary>
        /// Test lọc sản phẩm theo trạng thái
        /// Kiểm tra xem bộ lọc trạng thái có hoạt động đúng không
        /// </summary>
        [Test]
        public async Task GetFilteredAsync_WithStatusFilter_ReturnsStatusProducts()
        {
            // Act
            var (items, total) = await _repo.GetFilteredAsync(null, null, 1, null, null, null, null, 1, 20);

            // Assert
            Assert.That(total, Is.EqualTo(2)); // Cả 2 sản phẩm đều có status 1
        }

        /// <summary>
        /// Test sắp xếp sản phẩm theo giá tăng dần
        /// Kiểm tra xem sắp xếp có hoạt động đúng không
        /// </summary>
        [Test]
        public async Task GetFilteredAsync_WithPriceSortAsc_ReturnsSortedProducts()
        {
            // Act
            var (items, total) = await _repo.GetFilteredAsync(null, null, null, null, null, null, "price_asc", 1, 20);

            // Assert
            var productList = items.ToList();
            Assert.That(productList[0].Price, Is.EqualTo(22000000m)); // Samsung rẻ hơn
            Assert.That(productList[1].Price, Is.EqualTo(25000000m)); // iPhone đắt hơn
        }

        /// <summary>
        /// Test sắp xếp sản phẩm theo giá giảm dần
        /// Kiểm tra xem sắp xếp có hoạt động đúng không
        /// </summary>
        [Test]
        public async Task GetFilteredAsync_WithPriceSortDesc_ReturnsSortedProducts()
        {
            // Act
            var (items, total) = await _repo.GetFilteredAsync(null, null, null, null, null, null, "price_desc", 1, 20);

            // Assert
            var productList = items.ToList();
            Assert.That(productList[0].Price, Is.EqualTo(25000000m)); // iPhone đắt nhất đầu tiên
            Assert.That(productList[1].Price, Is.EqualTo(22000000m)); // Samsung rẻ hơn
        }

        /// <summary>
        /// Test phân trang sản phẩm
        /// Kiểm tra xem phân trang có hoạt động đúng không
        /// </summary>
        [Test]
        public async Task GetFilteredAsync_WithPaging_ReturnsPagedResults()
        {
            // Act - Lấy trang đầu tiên với 1 sản phẩm
            var (items, total) = await _repo.GetFilteredAsync(null, null, null, null, null, null, null, 1, 1);

            // Assert
            Assert.That(items.Count(), Is.EqualTo(1));
            Assert.That(total, Is.EqualTo(2)); // Tổng số vẫn là 2
        }

        /// <summary>
        /// Test kiểm tra sản phẩm có import pending
        /// Kiểm tra xem phương thức có trả về đúng trạng thái import không
        /// </summary>
        [Test]
        public async Task HasPendingImportAsync_ReturnsFalse_WhenNoPendingImports()
        {
            // Act
            var result = await _repo.HasPendingImportAsync(1);

            // Assert
            Assert.That(result, Is.False);
        }

        /// <summary>
        /// Test cập nhật thông tin sản phẩm
        /// Kiểm tra xem sản phẩm có được cập nhật đúng không
        /// </summary>
        [Test]
        public async Task UpdateAsync_UpdatesProductSuccessfully()
        {
            // Arrange
            var product = await _context.Products.FindAsync(1);
            product.Name = "iPhone 15 Updated";
            product.Price = 26000000m;

            // Act
            await _repo.UpdateAsync(product);

            // Assert
            var updated = await _context.Products.FindAsync(1);
            Assert.That(updated.Name, Is.EqualTo("iPhone 15 Updated"));
            Assert.That(updated.Price, Is.EqualTo(26000000m));
        }

        /// <summary>
        /// Test lấy dữ liệu ảnh sản phẩm khi ảnh tồn tại
        /// Kiểm tra xem có trả về dữ liệu byte array không
        /// </summary>
        [Test]
        public async Task GetImageBytesAsync_ReturnsImageData_WhenImageExists()
        {
            // Arrange - Tạo ảnh mẫu
            var image = new ProductImage
            {
                ImageId = 1,
                ProductId = 1,
                ImagePath = new byte[] { 0x89, 0x50, 0x4E, 0x47 } // PNG signature
            };
            _context.ProductImages.Add(image);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repo.GetImageBytesAsync(1);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.EqualTo(4));
        }

        /// <summary>
        /// Test lấy dữ liệu ảnh sản phẩm khi ảnh không tồn tại
        /// Kiểm tra xem có trả về null không
        /// </summary>
        [Test]
        public async Task GetImageBytesAsync_ReturnsNull_WhenImageNotExists()
        {
            // Act
            var result = await _repo.GetImageBytesAsync(999);

            // Assert
            Assert.That(result, Is.Null);
        }

        /// <summary>
        /// Test lưu ảnh chính cho sản phẩm
        /// Kiểm tra xem ảnh có được tạo và gán cho sản phẩm đúng không
        /// </summary>
        [Test]
        public async Task SavePrimaryImageAsync_CreatesImageAndUpdatesProduct()
        {
            // Arrange
            var imageData = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }; // PNG file

            // Act
            var imageId = await _repo.SavePrimaryImageAsync(1, imageData, "image/png");

            // Assert
            Assert.That(imageId, Is.GreaterThan(0));

            var product = await _context.Products.FindAsync(1);
            Assert.That(product.ImageId, Is.EqualTo(imageId));

            var image = await _context.ProductImages.FindAsync(imageId);
            Assert.That(image, Is.Not.Null);
            Assert.That(image.ImagePath, Is.EqualTo(imageData));
            Assert.That(image.IsPrimary, Is.True);
        }

        /// <summary>
        /// Test lấy tất cả trạng thái sản phẩm
        /// Kiểm tra xem có trả về danh sách trạng thái không
        /// </summary>
        [Test]
        public async Task GetAllStatusesAsync_ReturnsAllStatuses()
        {
            // Act
            var result = await _repo.GetAllStatusesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1)); // Chỉ có 1 status được seed
        }

        /// <summary>
        /// Test lấy tất cả danh mục sản phẩm
        /// Kiểm tra xem có trả về danh sách danh mục không
        /// </summary>
        [Test]
        public async Task GetAllCategoriesAsync_ReturnsAllCategories()
        {
            // Act
            var result = await _repo.GetAllCategoriesAsync();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count(), Is.EqualTo(1)); // Chỉ có 1 category được seed
        }

        /// <summary>
        /// Test logic xác định sản phẩm có thể chỉnh sửa
        /// Kiểm tra các điều kiện: có inventory, không có export history, không có pending import
        /// </summary>
        [Test]
        public async Task CanEditProduct_Logic_CorrectlyDeterminesEditableStatus()
        {
            // Arrange - Tạo sản phẩm với inventory (có thể edit)
            var product = new Product
            {
                ProductId = 3,
                Name = "Test Product",
                CategoryId = 1,
                StatusId = 1,
                Inventory = new List<Inventory>
                {
                    new Inventory { InventoryId = 1, ProductId = 3 }
                }
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act - Lấy sản phẩm với đầy đủ thông tin
            var result = await _repo.GetByIdWithIncludesAsync(3);

            // Assert - Kiểm tra qua repository method
            var listItem = await _repo.GetListItemByIdAsync(3);
            Assert.That(listItem.IsEditable, Is.True);
        }
    }
}