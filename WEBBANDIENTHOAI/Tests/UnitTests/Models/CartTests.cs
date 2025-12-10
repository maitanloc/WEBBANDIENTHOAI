using NUnit.Framework; // Sử dụng thư viện kiểm thử NUnit
using System; // Sử dụng các kiểu dữ liệu và hàm cơ bản của .NET
using System.Collections.Generic; // Sử dụng các kiểu danh sách (List)
using WEBBANDIENTHOAI.Models; // Sử dụng các model của dự án, bao gồm cả Cart

namespace WEBBANDIENTHOAI.Tests.UnitTests.Models
{
    [TestFixture] // Đánh dấu đây là một lớp chứa các bài kiểm thử (test case)
    public class CartTests
    {
        [Test] // Đánh dấu đây là một bài kiểm thử cụ thể
        public void Cart_CanBeInstantiated()
        {
            // Sắp xếp (Arrange): Chuẩn bị dữ liệu và đối tượng cần thiết
            var cart = new Cart(); // Tạo một đối tượng Giỏ hàng mới

            // Hành động & Khẳng định (Act & Assert): Kiểm tra xem đối tượng vừa tạo có null (rỗng) hay không
            Assert.That(cart, Is.Not.Null); // Khẳng định rằng giỏ hàng không phải là null, tức là đã được tạo thành công
        }

        [Test] // Đánh dấu một bài kiểm thử khác
        public void Cart_Properties_CanBeSetAndGet()
        {
            // Sắp xếp (Arrange): Chuẩn bị dữ liệu mẫu để kiểm tra
            var createdAt = new DateTime(2023, 1, 15, 10, 30, 0, DateTimeKind.Utc); // Tạo một ngày tạo mẫu
            var updatedAt = new DateTime(2023, 1, 15, 11, 0, 0, DateTimeKind.Utc); // Tạo một ngày cập nhật mẫu
            var customer = new Customer { CustomerId = 1, FullName = "Test Customer" }; // Tạo một khách hàng mẫu
            var cartDetails = new List<CartDetail> { new CartDetail { CartDetailId = 1 } }; // Tạo một danh sách chi tiết giỏ hàng mẫu

            // Tạo một đối tượng Giỏ hàng với các giá trị đã chuẩn bị
            var cart = new Cart
            {
                CartId = 1, // Gán ID cho giỏ hàng
                CustomerId = 101, // Gán ID khách hàng
                CreatedAt = createdAt, // Gán ngày tạo
                UpdatedAt = updatedAt, // Gán ngày cập nhật
                Customer = customer, // Gán đối tượng khách hàng
                Details = cartDetails // Gán danh sách chi tiết
            };

            // Khẳng định (Assert): Kiểm tra xem các thuộc tính đã được gán đúng giá trị hay chưa
            Assert.That(cart.CartId, Is.EqualTo(1)); // Kiểm tra ID giỏ hàng có bằng 1 không
            Assert.That(cart.CustomerId, Is.EqualTo(101)); // Kiểm tra ID khách hàng có bằng 101 không
            Assert.That(cart.CreatedAt, Is.EqualTo(createdAt)); // Kiểm tra ngày tạo có đúng không
            Assert.That(cart.UpdatedAt, Is.EqualTo(updatedAt)); // Kiểm tra ngày cập nhật có đúng không
            Assert.That(cart.Customer, Is.EqualTo(customer)); // Kiểm tra đối tượng khách hàng có đúng không
            Assert.That(cart.Details, Is.EqualTo(cartDetails)); // Kiểm tra danh sách chi tiết có đúng không
        }

        [Test] // Đánh dấu một bài kiểm thử khác
        public void Cart_DefaultValuesAreCorrect()
        {
            // Sắp xếp (Arrange): Tạo một giỏ hàng mới không có thông tin ban đầu
            var cart = new Cart();

            // Khẳng định (Assert): Kiểm tra các giá trị mặc định của giỏ hàng
            // Kiểm tra ngày tạo có phải là thời gian hiện tại không (với sai số 5 giây)
            Assert.That(cart.CreatedAt, Is.EqualTo(DateTime.UtcNow).Within(TimeSpan.FromSeconds(5)));
            Assert.That(cart.UpdatedAt, Is.Null); // Kiểm tra ngày cập nhật mặc định là null
            Assert.That(cart.Customer, Is.Null); // Kiểm tra khách hàng mặc định là null
            Assert.That(cart.Details, Is.Null); // Kiểm tra danh sách chi tiết mặc định là null
        }

        [Test] // Đánh dấu một bài kiểm thử khác
        public void Cart_Details_CanBeInitializedEmpty()
        {
            // Sắp xếp (Arrange):
            var cart = new Cart(); // Tạo một giỏ hàng mới
            cart.Details = new List<CartDetail>(); // Khởi tạo một danh sách chi tiết rỗng cho giỏ hàng

            // Khẳng định (Assert):
            Assert.That(cart.Details, Is.Not.Null); // Kiểm tra xem danh sách chi tiết có null không (đã được khởi tạo)
            Assert.That(cart.Details, Is.Empty); // Kiểm tra xem danh sách chi tiết có rỗng không (chưa có phần tử nào)
        }
    }
}