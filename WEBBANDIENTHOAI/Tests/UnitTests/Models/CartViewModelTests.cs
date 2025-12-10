using NUnit.Framework; // Sử dụng thư viện kiểm thử NUnit
using System.Collections.Generic; // Sử dụng các kiểu danh sách (List)
using System.Linq; // Sử dụng các hàm mở rộng cho danh sách như Sum()
using WEBBANDIENTHOAI.Models; // Sử dụng các model của dự án

namespace WEBBANDIENTHOAI.Tests.UnitTests.Models
{
    [TestFixture] // Đánh dấu đây là một lớp chứa các bài kiểm thử
    public class CartViewModelTests
    {
        [Test] // Đánh dấu một bài kiểm thử cụ thể
        public void CartViewModel_CanBeInstantiated()
        {
            // Sắp xếp (Arrange)
            var cartViewModel = new CartViewModel(); // Tạo một đối tượng CartViewModel mới

            // Khẳng định (Assert)
            Assert.That(cartViewModel, Is.Not.Null); // Kiểm tra xem đối tượng có được tạo thành công không
            Assert.That(cartViewModel.Items, Is.Not.Null); // Kiểm tra xem danh sách các sản phẩm (Items) có được khởi tạo không
            Assert.That(cartViewModel.Items, Is.Empty); // Kiểm tra xem danh sách các sản phẩm ban đầu có rỗng không
        }

        [Test] // Đánh dấu một bài kiểm thử khác
        public void CartViewModel_Properties_CanBeSetAndGet()
        {
            // Sắp xếp (Arrange): Chuẩn bị dữ liệu mẫu
            var item1 = new CartItemViewModel { ProductId = 1, Quantity = 2, Price = 100m }; // Tạo sản phẩm 1
            var item2 = new CartItemViewModel { ProductId = 2, Quantity = 1, Price = 50m }; // Tạo sản phẩm 2
            var items = new List<CartItemViewModel> { item1, item2 }; // Tạo danh sách chứa 2 sản phẩm trên

            // Tạo một CartViewModel với dữ liệu mẫu
            var cartViewModel = new CartViewModel
            {
                CartId = 1, // Gán ID giỏ hàng
                Items = items, // Gán danh sách sản phẩm
                Discount = 10m, // Gán tiền giảm giá
                ShippingFee = 5m // Gán phí vận chuyển
            };

            // Khẳng định (Assert): Kiểm tra các thuộc tính đã được gán đúng
            Assert.That(cartViewModel.CartId, Is.EqualTo(1)); // Kiểm tra ID giỏ hàng
            Assert.That(cartViewModel.Items, Is.EqualTo(items)); // Kiểm tra danh sách sản phẩm
            Assert.That(cartViewModel.Discount, Is.EqualTo(10m)); // Kiểm tra tiền giảm giá
            Assert.That(cartViewModel.ShippingFee, Is.EqualTo(5m)); // Kiểm tra phí vận chuyển
        }

        [Test] // Đánh dấu một bài kiểm thử khác, tập trung vào các thuộc tính tính toán
        public void CartViewModel_CalculatedPropertiesAreCorrect()
        {
            // Sắp xếp (Arrange): Chuẩn bị dữ liệu mẫu
            var item1 = new CartItemViewModel { ProductId = 1, Quantity = 2, Price = 100m }; // Sản phẩm 1 có tổng giá 200
            var item2 = new CartItemViewModel { ProductId = 2, Quantity = 1, Price = 50m };   // Sản phẩm 2 có tổng giá 50
            var items = new List<CartItemViewModel> { item1, item2 }; // Danh sách sản phẩm

            // Tạo CartViewModel với dữ liệu trên
            var cartViewModel = new CartViewModel
            {
                Items = items, // Gán danh sách
                Discount = 10m, // Giảm giá 10
                ShippingFee = 5m // Phí ship 5
            };

            // Khẳng định (Assert): Kiểm tra các giá trị được tính toán tự động
            Assert.That(cartViewModel.SubTotal, Is.EqualTo(250m)); // Kiểm tra Tạm tính = 200 + 50 = 250
            Assert.That(cartViewModel.Total, Is.EqualTo(245m));    // Kiểm tra Tổng cộng = 250 (Tạm tính) - 10 (Giảm giá) + 5 (Phí ship) = 245
            Assert.That(cartViewModel.TotalItems, Is.EqualTo(3));  // Kiểm tra Tổng số lượng = 2 + 1 = 3
        }

        [Test] // Đánh dấu một bài kiểm thử cho CartItemViewModel
        public void CartItemViewModel_CanBeInstantiated()
        {
            // Sắp xếp (Arrange)
            var cartItemViewModel = new CartItemViewModel(); // Tạo một đối tượng sản phẩm trong view model

            // Khẳng định (Assert)
            Assert.That(cartItemViewModel, Is.Not.Null); // Kiểm tra đối tượng được tạo thành công
            Assert.That(cartItemViewModel.IsSelected, Is.True); // Kiểm tra giá trị mặc định của IsSelected là true (được chọn)
        }

        [Test] // Đánh dấu một bài kiểm thử khác cho CartItemViewModel
        public void CartItemViewModel_Properties_CanBeSetAndGet()
        {
            // Sắp xếp (Arrange): Chuẩn bị dữ liệu mẫu cho một sản phẩm
            var cartItemViewModel = new CartItemViewModel
            {
                CartDetailId = 10,
                ProductId = 100,
                ProductName = "Test Phone",
                ImageUrl = "http://example.com/image.jpg",
                Color = "Black",
                Size = "Standard",
                Price = 500m,
                Quantity = 2,
                IsSelected = false
            };

            // Khẳng định (Assert): Kiểm tra các thuộc tính đã gán
            Assert.That(cartItemViewModel.CartDetailId, Is.EqualTo(10));
            Assert.That(cartItemViewModel.ProductId, Is.EqualTo(100));
            Assert.That(cartItemViewModel.ProductName, Is.EqualTo("Test Phone"));
            Assert.That(cartItemViewModel.ImageUrl, Is.EqualTo("http://example.com/image.jpg"));
            Assert.That(cartItemViewModel.Color, Is.EqualTo("Black"));
            Assert.That(cartItemViewModel.Size, Is.EqualTo("Standard"));
            Assert.That(cartItemViewModel.Price, Is.EqualTo(500m));
            Assert.That(cartItemViewModel.Quantity, Is.EqualTo(2));
            Assert.That(cartItemViewModel.IsSelected, Is.False);
        }

        [Test] // Đánh dấu một bài kiểm thử tính toán cho CartItemViewModel
        public void CartItemViewModel_CalculatedTotalIsCorrect()
        {
            // Sắp xếp (Arrange):
            var cartItemViewModel = new CartItemViewModel
            {
                Price = 500m, // Gán giá
                Quantity = 2 // Gán số lượng
            };

            // Khẳng định (Assert):
            Assert.That(cartItemViewModel.Total, Is.EqualTo(1000m)); // Kiểm tra tổng tiền của sản phẩm = 500 * 2 = 1000
        }
    }
}