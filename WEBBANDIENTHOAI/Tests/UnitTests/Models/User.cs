using NUnit.Framework;
using WEBBANDIENTHOAI.Models;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace WEBBANDIENTHOAI.Tests.Models
{
    [TestFixture]
    public class UserModelValidationTests
    {
        // Test 1: Kiểm tra Username bắt buộc
        [Test]
        public void KiemTra_Username_BatBuoc_KhongDuocDeTrong()
        {
            // Arrange
            var user = new User
            {
                Username = "",
                PasswordHash = new byte[] { 1, 2, 3 },
                FullName = "Nguyễn Văn A",
                Email = "test@example.com",
                RoleId = 1
            };

            var validationContext = new ValidationContext(user);
            var validationResults = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(user, validationContext, validationResults, true);

            // Assert
            Assert.That(isValid, Is.False, "Username là bắt buộc");
        }

        // Test 2: Kiểm tra PasswordHash bắt buộc
        [Test]
        public void KiemTra_PasswordHash_BatBuoc_KhongDuocDeTrong()
        {
            // Arrange
            var user = new User
            {
                Username = "testuser",
                PasswordHash = null,
                FullName = "Nguyễn Văn A",
                Email = "test@example.com",
                RoleId = 1
            };

            var validationContext = new ValidationContext(user);
            var validationResults = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(user, validationContext, validationResults, true);

            // Assert
            Assert.That(isValid, Is.False, "PasswordHash là bắt buộc");
        }

        // Test 3: Kiểm tra Username không vượt quá 100 ký tự
        [Test]
        public void KiemTra_Username_KhongVuotQua100KyTu()
        {
            // Arrange
            var user = new User
            {
                Username = new string('A', 101),
                PasswordHash = new byte[] { 1, 2, 3 },
                FullName = "Nguyễn Văn A",
                Email = "test@example.com",
                RoleId = 1
            };

            var validationContext = new ValidationContext(user);
            var validationResults = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(user, validationContext, validationResults, true);

            // Assert
            Assert.That(isValid, Is.False, "Username không được vượt quá 100 ký tự");
        }
    }
}