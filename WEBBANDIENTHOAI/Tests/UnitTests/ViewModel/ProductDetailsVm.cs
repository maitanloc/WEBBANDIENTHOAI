using NUnit.Framework;
using WEBBANDIENTHOAI.ViewModels;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace WEBBANDIENTHOAI.Tests.ViewModels
{
    [TestFixture]
    public class ProductDetailsVmValidationTests
    {
        // Test 1: Kiểm tra giá trị mặc định
        [Test]
        public void KiemTra_GiaTriMacDinh_Cua_ProductDetailsVm()
        {
            // Arrange & Act
            var vm = new ProductDetailsVm();

            // Assert - Sử dụng Assert.That
            Assert.Multiple(() =>
            {
                Assert.That(vm.SKU, Is.EqualTo(""));
                Assert.That(vm.Name, Is.EqualTo(""));
                Assert.That(vm.StockCode, Is.EqualTo(""));
                Assert.That(vm.Images, Is.Not.Null);
                Assert.That(vm.Images, Has.Count.EqualTo(0));
            });
        }

        // Test 2: Kiểm tra Price không âm
        [Test]
        public void KiemTra_Price_KhongDuocAm()
        {
            // Arrange
            var vm = new ProductDetailsVm
            {
                ProductId = 1,
                SKU = "TEST123",
                Name = "Test Product",
                Price = -100,
                StockCode = "STOCK001"
            };

            // Act
            var validationContext = new ValidationContext(vm);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(vm, validationContext, validationResults, true);

            // Assert
            Assert.That(isValid, Is.False, "Giá sản phẩm không được âm");
        }

        // Test 3: Kiểm tra OldPrice có thể null
        [Test]
        public void KiemTra_OldPrice_CoTheNull()
        {
            // Arrange
            var vm = new ProductDetailsVm
            {
                ProductId = 1,
                SKU = "TEST123",
                Name = "Test Product",
                Price = 1000000,
                OldPrice = null,
                StockCode = "STOCK001"
            };

            // Act & Assert
            Assert.That(vm.OldPrice, Is.Null);
        }
    }
}