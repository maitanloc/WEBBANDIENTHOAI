using NUnit.Framework;
using System.ComponentModel.DataAnnotations;
using WEBBANDIENTHOAI.Models;
using System.Collections.Generic;

namespace WEBBANDIENTHOAI.Tests.Models
{
    [TestFixture]
    public class ProductModelValidationTests
    {
        private Product _product;
        private ValidationContext _validationContext;
        private List<ValidationResult> _validationResults;

        [SetUp]
        public void Setup()
        {
            _product = new Product();
            _validationContext = new ValidationContext(_product);
            _validationResults = new List<ValidationResult>();
        }

        // Test 1: Kiểm tra khi CategoryId = 0 (không hợp lệ)
        [Test]
        public void KiemTra_CategoryId_BatBuoc_KhongDuocDeTrong()
        {
            // Arrange
            _product.CategoryId = 0;
            _product.SKU = "TEST123";
            _product.Name = "Test Product";
            _product.StockCode = "STOCK001";

            // Act
            var isValid = Validator.TryValidateObject(_product, _validationContext, _validationResults, true);

            // Assert - Sử dụng Assert.That
            Assert.That(isValid, Is.False, "CategoryId bắt buộc không được để trống");
        }

        // Test 2: Kiểm tra SKU bắt buộc và độ dài tối đa
        [Test]
        public void KiemTra_SKU_BatBuoc_Va_KhongVuotQua60KyTu()
        {
            // Arrange
            _product.CategoryId = 1;
            _product.SKU = new string('A', 61);
            _product.Name = "Test Product";
            _product.StockCode = "STOCK001";

            // Act
            var isValid = Validator.TryValidateObject(_product, _validationContext, _validationResults, true);

            // Assert
            Assert.That(isValid, Is.False, "SKU không được vượt quá 60 ký tự");
        }

        // Test 3: Kiểm tra Name bắt buộc và độ dài tối đa
        [Test]
        public void KiemTra_Name_BatBuoc_Va_KhongVuotQua250KyTu()
        {
            // Arrange
            _product.CategoryId = 1;
            _product.SKU = "TEST123";
            _product.Name = new string('A', 251);
            _product.StockCode = "STOCK001";

            // Act
            var isValid = Validator.TryValidateObject(_product, _validationContext, _validationResults, true);

            // Assert
            Assert.That(isValid, Is.False, "Tên sản phẩm không được vượt quá 250 ký tự");
        }

        // Test 4: Kiểm tra Brand không vượt quá 100 ký tự
        [Test]
        public void KiemTra_Brand_KhongVuotQua100KyTu()
        {
            // Arrange
            _product.CategoryId = 1;
            _product.SKU = "TEST123";
            _product.Name = "Test Product";
            _product.StockCode = "STOCK001";
            _product.Brand = new string('A', 101);

            // Act
            var isValid = Validator.TryValidateObject(_product, _validationContext, _validationResults, true);

            // Assert
            Assert.That(isValid, Is.False, "Thương hiệu không được vượt quá 100 ký tự");
        }

        // Test 5: Kiểm tra StockCode bắt buộc và độ dài
        [Test]
        public void KiemTra_StockCode_BatBuoc_Va_KhongVuotQua50KyTu()
        {
            // Arrange
            _product.CategoryId = 1;
            _product.SKU = "TEST123";
            _product.Name = "Test Product";
            _product.StockCode = new string('A', 51);

            // Act
            var isValid = Validator.TryValidateObject(_product, _validationContext, _validationResults, true);

            // Assert
            Assert.That(isValid, Is.False, "Mã kho không được vượt quá 50 ký tự");
        }

        // Test 6: Kiểm tra dữ liệu hợp lệ
        [Test]
        public void KiemTra_Product_HopLe_Voi_DuLieuChinhXac()
        {
            // Arrange
            _product.CategoryId = 1;
            _product.SKU = "IPHONE15-128GB";
            _product.Name = "iPhone 15 128GB";
            _product.Brand = "Apple";
            _product.Price = 24990000;
            _product.StockCode = "APPLE-001";
            _product.Color = "Đen";
            _product.Size = "6.1 inch";
            _product.ShortDescription = "iPhone 15 mới nhất";

            // Act
            var isValid = Validator.TryValidateObject(_product, _validationContext, _validationResults, true);

            // Assert
            Assert.That(isValid, Is.True, "Product phải hợp lệ với dữ liệu chính xác");
            Assert.That(_validationResults, Has.Count.EqualTo(0), "Không có lỗi validation");
        }
    }
}