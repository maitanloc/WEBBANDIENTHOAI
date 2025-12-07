using NUnit.Framework;
using System.ComponentModel.DataAnnotations;
using WEBBANDIENTHOAI.Models;
using System.Collections.Generic;

namespace WEBBANDIENTHOAI.Tests.Models
{
    [TestFixture]
    public class CreateProductStatusDtoValidationTests
    {
        private CreateProductStatusDto _dto;
        private ValidationContext _validationContext;
        private List<ValidationResult> _validationResults;

        [SetUp]
        public void Setup()
        {
            _dto = new CreateProductStatusDto();
            _validationContext = new ValidationContext(_dto);
            _validationResults = new List<ValidationResult>();
        }

        // Test 1: Kiểm tra StatusName bắt buộc
        [Test]
        public void KiemTra_StatusName_BatBuoc_KhongDuocDeTrong()
        {
            // Arrange
            _dto.StatusName = "";
            _dto.Description = "Mô tả trạng thái";

            // Act
            var isValid = Validator.TryValidateObject(_dto, _validationContext, _validationResults, true);

            // Assert
            Assert.That(isValid, Is.False, "Tên trạng thái là bắt buộc");
            Assert.That(_validationResults[0].ErrorMessage, Does.Contain("Tên trạng thái là bắt buộc"));
        }

        // Test 2: Kiểm tra StatusName không vượt quá 30 ký tự
        [Test]
        public void KiemTra_StatusName_KhongVuotQua30KyTu()
        {
            // Arrange
            _dto.StatusName = new string('A', 31);
            _dto.Description = "Mô tả";

            // Act
            var isValid = Validator.TryValidateObject(_dto, _validationContext, _validationResults, true);

            // Assert
            Assert.That(isValid, Is.False, "Tên trạng thái không được quá 30 ký tự");
            Assert.That(_validationResults[0].ErrorMessage, Does.Contain("Tên trạng thái không quá 30 ký tự"));
        }

        // Test 3: Kiểm tra Description không vượt quá 150 ký tự
        [Test]
        public void KiemTra_Description_KhongVuotQua150KyTu()
        {
            // Arrange
            _dto.StatusName = "Đang hoạt động";
            _dto.Description = new string('A', 151);

            // Act
            var isValid = Validator.TryValidateObject(_dto, _validationContext, _validationResults, true);

            // Assert
            Assert.That(isValid, Is.False, "Mô tả không được quá 150 ký tự");
            Assert.That(_validationResults[0].ErrorMessage, Does.Contain("Mô tả không quá 150 ký tự"));
        }

        // Test 4: Kiểm tra dữ liệu hợp lệ
        [Test]
        public void KiemTra_CreateProductStatusDto_HopLe_Voi_DuLieuChinhXac()
        {
            // Arrange
            _dto.StatusName = "Ngừng kinh doanh";
            _dto.Description = "Sản phẩm tạm ngừng kinh doanh";

            // Act
            var isValid = Validator.TryValidateObject(_dto, _validationContext, _validationResults, true);

            // Assert
            Assert.That(isValid, Is.True, "DTO phải hợp lệ với dữ liệu chính xác");
            Assert.That(_validationResults, Is.Empty);
        }
    }
}