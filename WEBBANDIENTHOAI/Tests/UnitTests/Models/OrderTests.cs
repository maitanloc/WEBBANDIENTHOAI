using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Tests.UnitTests.Models
{
    [TestFixture]
    public class OrderTests
    {
        [Test]
        public void Order_CanBeInstantiated()
        {
            var order = new Order();
            Assert.That(order, Is.Not.Null);
        }

        [Test]
        public void Order_Properties_CanBeSetAndGet()
        {
            // Arrange
            var order = new Order
            {
                OrderId = 1,
                CustomerId = 101,
                OrderDate = new DateTime(2023, 1, 15, 10, 30, 0, DateTimeKind.Utc),
                Total = 99.99m,
                StatusId = 2, // Processing
                ShippingAddress = "123 Main St, Anytown",
                CreatedByUserId = 1,
                PaymentMethod = "Credit Card",
                Notes = "Fragile items"
            };

            // Assert
            Assert.That(order.OrderId, Is.EqualTo(1));
            Assert.That(order.CustomerId, Is.EqualTo(101));
            Assert.That(order.OrderDate, Is.EqualTo(new DateTime(2023, 1, 15, 10, 30, 0, DateTimeKind.Utc)));
            Assert.That(order.Total, Is.EqualTo(99.99m));
            Assert.That(order.StatusId, Is.EqualTo(2));
            Assert.That(order.ShippingAddress, Is.EqualTo("123 Main St, Anytown"));
            Assert.That(order.CreatedByUserId, Is.EqualTo(1));
            Assert.That(order.PaymentMethod, Is.EqualTo("Credit Card"));
            Assert.That(order.Notes, Is.EqualTo("Fragile items"));
        }

        [Test]
        public void Order_DefaultValuesAreCorrect()
        {
            // Arrange
            var order = new Order();

            // Assert
            Assert.That(order.OrderDate, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5))); // Check within a small time frame
            Assert.That(order.Total, Is.EqualTo(0m));
            Assert.That(order.StatusId, Is.EqualTo(1)); // Default to Pending
            Assert.That(order.PaymentMethod, Is.EqualTo("COD")); // Default to COD
            Assert.That(order.Notes, Is.Null);
            Assert.That(order.ShippingAddress, Is.Null); // Ensure it's null by default if not set
            Assert.That(order.CreatedByUserId, Is.Null); // Ensure it's null by default if not set
        }

        [Test]
        public void Order_NavigationProperties_CanBeInitialized()
        {
            // Arrange
            var order = new Order();
            order.Customer = new Customer { CustomerId = 101, FullName = "Test Customer" };
            order.OrderStatus = new OrderStatus { StatusId = 1, StatusName = "Pending" };
            order.OrderDetails = new List<OrderDetail> { new OrderDetail { ProductId = 1, Quantity = 2 } };

            // Assert
            Assert.That(order.Customer, Is.Not.Null);
            Assert.That(order.Customer.CustomerId, Is.EqualTo(101));
            Assert.That(order.OrderStatus, Is.Not.Null);
            Assert.That(order.OrderStatus.StatusName, Is.EqualTo("Pending"));
            Assert.That(order.OrderDetails, Is.Not.Null);
            Assert.That(order.OrderDetails, Is.InstanceOf<ICollection<OrderDetail>>());
            Assert.That(order.OrderDetails.Count, Is.EqualTo(1));
        }

        [Test]
        public void OrderStatisticsDto_CanBeInstantiatedAndPropertiesSet()
        {
            var dto = new OrderStatisticsDto
            {
                TotalOrders = 10,
                TotalRevenue = 1500.50m,
                PendingOrders = 3,
                ProcessingOrders = 2,
                CompletedOrders = 5,
                CancelledOrders = 0
            };

            Assert.That(dto, Is.Not.Null);
            Assert.That(dto.TotalOrders, Is.EqualTo(10));
            Assert.That(dto.TotalRevenue, Is.EqualTo(1500.50m));
            Assert.That(dto.PendingOrders, Is.EqualTo(3));
            Assert.That(dto.ProcessingOrders, Is.EqualTo(2));
            Assert.That(dto.CompletedOrders, Is.EqualTo(5));
            Assert.That(dto.CancelledOrders, Is.EqualTo(0));
        }

        [Test]
        public void OrderStatusUpdateDto_CanBeInstantiatedAndPropertiesSet()
        {
            var dto = new OrderStatusUpdateDto
            {
                StatusId = 3 // Completed
            };

            Assert.That(dto, Is.Not.Null);
            Assert.That(dto.StatusId, Is.EqualTo(3));
        }

        [Test]
        public void OrderStatusUpdateDto_StatusId_IsRequired()
        {
            var dto = new OrderStatusUpdateDto();
            var validationContext = new ValidationContext(dto);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(dto, validationContext, validationResults, true);

            Assert.That(isValid, Is.False);
            Assert.That(validationResults.Count, Is.GreaterThan(0));
            Assert.That(validationResults[0].ErrorMessage, Is.EqualTo("Vui lòng chọn trạng thái"));
        }
    }
}