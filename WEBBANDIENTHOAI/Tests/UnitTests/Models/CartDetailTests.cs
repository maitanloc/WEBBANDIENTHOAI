using NUnit.Framework; // Sử dụng thư viện kiểm thử NUnit
using System; // Sử dụng các kiểu dữ liệu và hàm cơ bản của .NET
using WEBBANDIENTHOAI.Models; // Sử dụng các model của dự án

namespace WEBBANDIENTHOAI.Tests.UnitTests.Models
{
    [TestFixture] // Đánh dấu đây là một lớp chứa các bài kiểm thử
    public class CartDetailTests
    {
        [Test] // Đánh dấu một bài kiểm thử cụ thể
        public void CartDetail_CanBeInstantiated()
        {
            // Sắp xếp (Arrange): Chuẩn bị
            var cartDetail = new CartDetail(); // Tạo một đối tượng Chi tiết giỏ hàng mới

            // Khẳng định (Assert): Kiểm tra
            Assert.That(cartDetail, Is.Not.Null); // Khẳng định rằng đối tượng không phải là null, tức là tạo thành công
        }

        [Test] // Đánh dấu một bài kiểm thử khác
        public void CartDetail_Properties_CanBeSetAndGet()
        {
            // Sắp xếp (Arrange): Chuẩn bị dữ liệu mẫu
            var cart = new Cart { CartId = 1, CustomerId = 101 }; // Tạo một giỏ hàng mẫu
            var product = new Product { ProductId = 10, Name = "Test Product" }; // Tạo một sản phẩm mẫu (Đã sửa từ Name thành ProductName)

            // Tạo một đối tượng Chi tiết giỏ hàng với dữ liệu đã chuẩn bị
            var cartDetail = new CartDetail
            {
                CartDetailId = 5, // Gán ID chi tiết
                CartId = 1, // Gán ID giỏ hàng
                ProductId = 10, // Gán ID sản phẩm
                Quantity = 3, // Gán số lượng
                UnitPrice = 150.75m, // Gán đơn giá
                Cart = cart, // Gán đối tượng giỏ hàng
                Product = product // Gán đối tượng sản phẩm
            };

            // Khẳng định (Assert): Kiểm tra xem các thuộc tính đã được gán đúng giá trị hay chưa
            Assert.That(cartDetail.CartDetailId, Is.EqualTo(5)); // Kiểm tra ID chi tiết có bằng 5 không
            Assert.That(cartDetail.CartId, Is.EqualTo(1)); // Kiểm tra ID giỏ hàng có bằng 1 không
            Assert.That(cartDetail.ProductId, Is.EqualTo(10)); // Kiểm tra ID sản phẩm có bằng 10 không
            Assert.That(cartDetail.Quantity, Is.EqualTo(3)); // Kiểm tra số lượng có bằng 3 không
            Assert.That(cartDetail.UnitPrice, Is.EqualTo(150.75m)); // Kiểm tra đơn giá có đúng không
            Assert.That(cartDetail.Cart, Is.EqualTo(cart)); // Kiểm tra đối tượng giỏ hàng có đúng không
            Assert.That(cartDetail.Product, Is.EqualTo(product)); // Kiểm tra đối tượng sản phẩm có đúng không
        }

        [Test] // Đánh dấu một bài kiểm thử khác
        public void CartDetail_DefaultValuesAreCorrect()
        {
            // Sắp xếp (Arrange):
            var cartDetail = new CartDetail(); // Tạo một chi tiết giỏ hàng mới không có thông tin ban đầu

            // Khẳng định (Assert): Kiểm tra các giá trị mặc định
            Assert.That(cartDetail.Quantity, Is.EqualTo(1)); // Kiểm tra số lượng mặc định có bằng 1 không
            Assert.That(cartDetail.UnitPrice, Is.EqualTo(0m)); // Kiểm tra đơn giá mặc định có bằng 0 không
            Assert.That(cartDetail.Cart, Is.Null); // Kiểm tra đối tượng giỏ hàng mặc định là null
            Assert.That(cartDetail.Product, Is.Null); // Kiểm tra đối tượng sản phẩm mặc định là null
        }
    }
}