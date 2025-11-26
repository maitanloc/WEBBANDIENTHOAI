// Tests/UnitTests/Repository/ProductRepositoryTests.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Repository;

namespace WEBBANDIENTHOAI.Tests.UnitTests.Repository
{
    [TestFixture]
    public class ProductRepositoryTests
    {
        private AppDbContext _context;
        private ProductRepository _repository;

        [SetUp]
        public void Setup()
        {
            // Sử dụng InMemory database cho tất cả test - tránh lỗi mock phức tạp
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_" + Guid.NewGuid())
                .Options;

            _context = new AppDbContext(options);
            _repository = new ProductRepository(_context);

            // Seed dữ liệu test cơ bản
            SeedTestData();
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
        }

        /// <summary>
        /// Chuẩn bị dữ liệu test cho tất cả test cases
        /// </summary>
        private void SeedTestData()
        {
            // Thêm danh mục test
            var categories = new List<Category>
            {
                new Category { CategoryId = 1, CategoryName = "Smartphones", Description = "Điện thoại" },
                new Category { CategoryId = 2, CategoryName = "Laptops", Description = "Máy tính" }
            };

            // Thêm trạng thái sản phẩm test
            var statuses = new List<ProductStatus>
            {
                new ProductStatus { StatusId = 1, StatusName = "InStock", Description = "Còn hàng" },
                new ProductStatus { StatusId = 2, StatusName = "OutOfStock", Description = "Hết hàng" }
            };

            // Thêm sản phẩm test
            var products = new List<Product>
            {
                new Product { ProductId = 1, Name = "iPhone 15", SKU = "IP15", StockCode = "STOCK001", Price = 25000000m, CategoryId = 1, StatusId = 1 },
                new Product { ProductId = 2, Name = "Samsung Galaxy", SKU = "SG24", StockCode = "STOCK002", Price = 20000000m, CategoryId = 1, StatusId = 1 },
                new Product { ProductId = 3, Name = "MacBook Pro", SKU = "MBP", StockCode = "STOCK003", Price = 45000000m, CategoryId = 2, StatusId = 1 }
            };

            _context.Categories.AddRange(categories);
            _context.ProductStatuses.AddRange(statuses);
            _context.Products.AddRange(products);
            _context.SaveChanges();
        }

        // ===============================================
        // TEST CÁC PHƯƠNG THỨC LẤY DANH SÁCH
        // ===============================================

        /// <summary>
        /// Test lấy tất cả trạng thái sản phẩm
        /// </summary>
        [Test]
        public async Task LayTatCaTrangThaiSanPham_TraVeDanhSachTrangThai()
        {
            // Act - Gọi phương thức lấy danh sách trạng thái
            var result = await _repository.GetAllStatusesAsync();

            // Assert - Kiểm tra kết quả trả về
            Assert.That(result, Is.Not.Null, "Danh sách trạng thái không được null");
            Assert.That(result.Count(), Is.EqualTo(2), "Số lượng trạng thái phải là 2");
            Assert.That(result.Any(s => s.StatusName == "InStock"), Is.True, "Phải có trạng thái 'InStock'");
        }

        /// <summary>
        /// Test lấy tất cả danh mục sản phẩm
        /// </summary>
        [Test]
        public async Task LayTatCaDanhMucSanPham_TraVeDanhSachDanhMuc()
        {
            // Act - Gọi phương thức lấy danh sách danh mục
            var result = await _repository.GetAllCategoriesAsync();

            // Assert - Kiểm tra kết quả trả về
            Assert.That(result, Is.Not.Null, "Danh sách danh mục không được null");
            Assert.That(result.Count(), Is.EqualTo(2), "Số lượng danh mục phải là 2");
            Assert.That(result.Any(c => c.CategoryName == "Smartphones"), Is.True, "Phải có danh mục 'Smartphones'");
        }

        // ===============================================
        // TEST CÁC PHƯƠNG THỨC LẤY DỮ LIỆU ẢNH
        // ===============================================

        /// <summary>
        /// Test lấy dữ liệu ảnh với ID hợp lệ
        /// </summary>
        [Test]
        public async Task LayDuLieuAnh_VoiIdHopLe_TraVeDuLieuAnh()
        {
            // Arrange - Chuẩn bị dữ liệu ảnh test
            var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
            var productImage = new ProductImage { ImageId = 1, ImagePath = imageBytes, ProductId = 1 };
            _context.ProductImages.Add(productImage);
            await _context.SaveChangesAsync();

            // Act - Gọi phương thức lấy dữ liệu ảnh
            var result = await _repository.GetImageBytesAsync(1);

            // Assert - Kiểm tra dữ liệu ảnh trả về
            Assert.That(result, Is.Not.Null, "Dữ liệu ảnh không được null");
            Assert.That(result, Is.EqualTo(imageBytes), "Dữ liệu ảnh phải khớp với dữ liệu đã lưu");
        }

        /// <summary>
        /// Test lấy dữ liệu ảnh với ID không tồn tại
        /// </summary>
        [Test]
        public async Task LayDuLieuAnh_VoiIdKhongTonTai_TraVeNull()
        {
            // Act - Gọi phương thức lấy dữ liệu ảnh với ID không tồn tại
            var result = await _repository.GetImageBytesAsync(999);

            // Assert - Kiểm tra kết quả trả về là null
            Assert.That(result, Is.Null, "Dữ liệu ảnh phải là null khi ID không tồn tại");
        }

        // ===============================================
        // TEST CÁC PHƯƠNG THỨC KIỂM TRA TRẠNG THÁI
        // ===============================================

        /// <summary>
        /// Test kiểm tra sản phẩm không có import đang chờ xử lý
        /// </summary>
        [Test]
        public async Task KiemTraImportDangCho_VoiSanPhamKhongCoImportCho_TraVeFalse()
        {
            // Act - Gọi phương thức kiểm tra import pending
            var result = await _repository.HasPendingImportAsync(1);

            // Assert - Kiểm tra kết quả trả về là false
            Assert.That(result, Is.False, "Phải trả về false khi sản phẩm không có import pending");
        }

        // ===============================================
        // TEST CÁC PHƯƠNG THỨC CẬP NHẬT
        // ===============================================

        /// <summary>
        /// Test cập nhật thông tin sản phẩm hợp lệ
        /// </summary>
        [Test]
        public async Task CapNhatSanPham_VoiDuLieuHopLe_CapNhatThanhCong()
        {
            // Arrange - Lấy sản phẩm từ database và sửa tên
            var product = await _context.Products.FindAsync(1);
            var originalName = product.Name;
            product.Name = "Tên đã cập nhật";

            // Act - Gọi phương thức cập nhật
            await _repository.UpdateAsync(product);

            // Assert - Kiểm tra dữ liệu đã được cập nhật trong database
            var updatedProduct = await _context.Products.FindAsync(1);
            Assert.That(updatedProduct.Name, Is.EqualTo("Tên đã cập nhật"), "Tên sản phẩm phải được cập nhật");
            Assert.That(updatedProduct.Name, Is.Not.EqualTo(originalName), "Tên sản phẩm không được giữ nguyên");
        }

        // ===============================================
        // TEST CÁC PHƯƠNG THỨC LẤY SẢN PHẨM
        // ===============================================

        /// <summary>
        /// Test lấy sản phẩm theo ID hợp lệ với đầy đủ thông tin
        /// </summary>
        [Test]
        public async Task LaySanPhamTheoId_VoiIdHopLe_TraVeSanPhamDayDuThongTin()
        {
            // Act - Gọi phương thức lấy sản phẩm theo ID
            var result = await _repository.GetByIdWithIncludesAsync(1);

            // Assert - Kiểm tra sản phẩm trả về
            Assert.That(result, Is.Not.Null, "Sản phẩm không được null");
            Assert.That(result.ProductId, Is.EqualTo(1), "ID sản phẩm phải khớp");
            Assert.That(result.Name, Is.EqualTo("iPhone 15"), "Tên sản phẩm phải khớp");
        }

        /// <summary>
        /// Test lấy sản phẩm theo ID không tồn tại
        /// </summary>
        [Test]
        public async Task LaySanPhamTheoId_VoiIdKhongTonTai_TraVeNull()
        {
            // Act - Gọi phương thức lấy sản phẩm với ID không tồn tại
            var result = await _repository.GetByIdWithIncludesAsync(999);

            // Assert - Kiểm tra kết quả trả về là null
            Assert.That(result, Is.Null, "Phải trả về null khi ID không tồn tại");
        }

        /// <summary>
        /// Test lấy thông tin sản phẩm dạng list item với ID hợp lệ
        /// </summary>
        [Test]
        public async Task LayThongTinSanPhamListItem_VoiIdHopLe_TraVeThongTinSanPham()
        {
            // Act - Gọi phương thức lấy thông tin sản phẩm dạng list item
            var result = await _repository.GetListItemByIdAsync(1);

            // Assert - Kiểm tra thông tin sản phẩm trả về
            Assert.That(result, Is.Not.Null, "Thông tin sản phẩm không được null");
            Assert.That(result.ProductId, Is.EqualTo(1), "ID sản phẩm phải khớp");
            Assert.That(result.Name, Is.EqualTo("iPhone 15"), "Tên sản phẩm phải khớp");
            Assert.That(result.SKU, Is.EqualTo("IP15"), "SKU sản phẩm phải khớp");
        }

        /// <summary>
        /// Test lấy thông tin sản phẩm dạng list item với ID không tồn tại
        /// </summary>
        [Test]
        public async Task LayThongTinSanPhamListItem_VoiIdKhongTonTai_TraVeNull()
        {
            // Act - Gọi phương thức lấy thông tin sản phẩm với ID không tồn tại
            var result = await _repository.GetListItemByIdAsync(999);

            // Assert - Kiểm tra kết quả trả về là null
            Assert.That(result, Is.Null, "Phải trả về null khi ID không tồn tại");
        }

        // ===============================================
        // TEST CÁC PHƯƠNG THỨC LỌC VÀ PHÂN TRANG
        // ===============================================

        /// <summary>
        /// Test lọc sản phẩm với từ khóa tìm kiếm
        /// </summary>
        [Test]
        public async Task LocSanPham_VoiTuKhoaTimKiem_TraVeSanPhamPhuHop()
        {
            // Act - Gọi phương thức lọc với từ khóa "iPhone"
            var (items, total) = await _repository.GetFilteredAsync("iPhone", null, null, null, null, null, null, 1, 10);

            // Assert - Kiểm tra kết quả lọc
            Assert.That(total, Is.EqualTo(1), "Chỉ có 1 sản phẩm khớp với từ khóa 'iPhone'");
            Assert.That(items.Count(), Is.EqualTo(1), "Danh sách trả về chỉ có 1 sản phẩm");
            Assert.That(items.First().Name, Is.EqualTo("iPhone 15"), "Sản phẩm trả về phải là iPhone 15");
        }

        /// <summary>
        /// Test lọc sản phẩm theo danh mục
        /// </summary>
        [Test]
        public async Task LocSanPham_TheoDanhMuc_TraVeSanPhamTrongDanhMuc()
        {
            // Act - Gọi phương thức lọc theo danh mục Laptops (ID = 2)
            var (items, total) = await _repository.GetFilteredAsync(null, 2, null, null, null, null, null, 1, 10);

            // Assert - Kiểm tra kết quả lọc
            Assert.That(total, Is.EqualTo(1), "Chỉ có 1 sản phẩm trong danh mục Laptops");
            Assert.That(items.First().Name, Is.EqualTo("MacBook Pro"), "Sản phẩm trả về phải là MacBook Pro");
        }

        /// <summary>
        /// Test phân trang sản phẩm
        /// </summary>
        [Test]
        public async Task PhanTrangSanPham_TraVeTrangDungVaTongSoLuong()
        {
            // Act - Gọi phương thức lọc với phân trang (trang 1, 1 sản phẩm/trang)
            var (items, total) = await _repository.GetFilteredAsync(null, null, null, null, null, null, null, 1, 1);

            // Assert - Kiểm tra kết quả phân trang
            Assert.That(total, Is.EqualTo(3), "Tổng số sản phẩm phải là 3");
            Assert.That(items.Count(), Is.EqualTo(1), "Chỉ trả về 1 sản phẩm cho trang đầu tiên");
        }

        // ===============================================
        // TEST CÁC PHƯƠNG THỨC LƯU ẢNH
        // ===============================================

        /// <summary>
        /// Test lưu ảnh chính cho sản phẩm hợp lệ
        /// </summary>
        [Test]
        public async Task LuuAnhChinhChoSanPham_VoiDuLieuHopLe_TraVeIdAnhVaCapNhatSanPham()
        {
            // Arrange - Chuẩn bị dữ liệu ảnh
            var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 };

            // Act - Gọi phương thức lưu ảnh chính
            var imageId = await _repository.SavePrimaryImageAsync(1, imageBytes, "image/png");

            // Assert - Kiểm tra kết quả lưu ảnh
            Assert.That(imageId, Is.GreaterThan(0), "ID ảnh phải lớn hơn 0");

            // Kiểm tra ảnh đã được lưu trong database
            var savedImage = await _context.ProductImages.FindAsync(imageId);
            Assert.That(savedImage, Is.Not.Null, "Ảnh phải được lưu trong database");
            Assert.That(savedImage.ImagePath, Is.EqualTo(imageBytes), "Dữ liệu ảnh phải khớp");
            Assert.That(savedImage.IsPrimary, Is.True, "Ảnh phải được đánh dấu là ảnh chính");

            // Kiểm tra sản phẩm đã được cập nhật ImageId
            var updatedProduct = await _context.Products.FindAsync(1);
            Assert.That(updatedProduct.ImageId, Is.EqualTo(imageId), "Sản phẩm phải được cập nhật ImageId");
        }

        /// <summary>
        /// Test lưu ảnh cho sản phẩm không tồn tại - phải ném exception
        /// </summary>
        [Test]
        public void LuuAnhChinhChoSanPham_VoiSanPhamKhongTonTai_NemException()
        {
            // Arrange - Chuẩn bị dữ liệu ảnh
            var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 };

            // Act & Assert - Kiểm tra phương thức ném exception khi sản phẩm không tồn tại
            var ex = Assert.ThrowsAsync<Exception>(async () =>
                await _repository.SavePrimaryImageAsync(999, imageBytes, "image/png"));

            Assert.That(ex.Message, Contains.Substring("Product with ID 999 not found"),
                "Thông báo lỗi phải chứa thông tin sản phẩm không tồn tại");
        }

        // ===============================================
        // TEST TÍNH NĂNG EDIT SẢN PHẨM
        // ===============================================

        /// <summary>
        /// Test kiểm tra sản phẩm không có inventory - không thể edit
        /// (Test gián tiếp thông qua GetListItemByIdAsync vì CanEditProduct là private)
        /// </summary>
        [Test]
        public async Task KiemTraSanPhamKhongCoInventory_KhongTheEdit()
        {
            // Arrange - Tạo sản phẩm mới không có inventory
            var product = new Product
            {
                ProductId = 100,
                Name = "Sản phẩm test không inventory",
                SKU = "TEST100",
                StockCode = "STOCK100",
                CategoryId = 1,
                StatusId = 1
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Act - Lấy thông tin sản phẩm dạng list item
            var listItem = await _repository.GetListItemByIdAsync(100);

            // Assert - Sản phẩm không có inventory nên không thể edit
            Assert.That(listItem.IsEditable, Is.False, "Sản phẩm không có inventory không thể edit");
        }
    }
}