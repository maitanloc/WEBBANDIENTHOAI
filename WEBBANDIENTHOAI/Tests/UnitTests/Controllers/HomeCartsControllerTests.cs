using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Repository.NguoiDung;

namespace WEBBANDIENTHOAI.Tests.Controllers
{
    /// <summary>
    /// Unit Tests cho Cart Repository & Controller – Kiểm tra 3 nghiệp vụ giỏ hàng nâng cao:
    /// 1. Tùy chọn cấu hình (Options)
    /// 2. Logic gộp/tách giỏ hàng theo cấu hình
    /// 3. Cross-selling
    /// </summary>
    [TestFixture]
    public class HomeCartsControllerTests
    {
        private AppDbContext _context;

        [SetUp]
        public void Setup()
        {
            // Dùng InMemory Database để test không cần SQL Server
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"CartTestDb_{System.Guid.NewGuid()}")
                .Options;
            _context = new AppDbContext(options);

            // Seed sản phẩm test
            _context.Products.AddRange(
                new Product { ProductId = 1, Name = "iPhone 15 Pro", Price = 28990000m, SKU = "IP15P", StockCode = "SC-IP15P", CategoryId = 1, StatusId = 1 },
                new Product { ProductId = 2, Name = "Op lung iPhone 15 Pro", Price = 199000m, SKU = "OL-IP15P", StockCode = "SC-OL", CategoryId = 1, StatusId = 1 }
            );
            // Seed options cho iPhone 15 Pro
            _context.ProductOptions.AddRange(
                new ProductOption { ProductOptionId = 1, ProductId = 1, GroupName = "Mau sac", OptionName = "Titan Xanh", AdditionalPrice = 0m },
                new ProductOption { ProductOptionId = 2, ProductId = 1, GroupName = "Mau sac", OptionName = "Titan Den", AdditionalPrice = 0m },
                new ProductOption { ProductOptionId = 3, ProductId = 1, GroupName = "Bo nho trong", OptionName = "256GB", AdditionalPrice = 0m },
                new ProductOption { ProductOptionId = 4, ProductId = 1, GroupName = "Bo nho trong", OptionName = "512GB", AdditionalPrice = 3000000m }
            );
            // Seed quy tắc cross-sell
            _context.CrossSellRules.Add(new CrossSellRule
            {
                CrossSellRuleId = 1,
                TriggerProductId = 1,
                SuggestedProductId = 2,
                DiscountedPrice = 99000m,
                DisplayMessage = "Hoan thien trai nghiem iPhone cua ban!",
                IsActive = true,
                CreatedAt = System.DateTime.UtcNow
            });
            _context.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        // ─────────────────────────────────────────────────────────────────────────
        // TEST 1: Gộp dòng – Cùng ProductId + Cùng cấu hình → SL tăng, vẫn 1 dòng
        // ─────────────────────────────────────────────────────────────────────────
        [Test]
        public async Task AddToCart_GopDong_KhiCungProductId_VaCungOptionsJson()
        {
            // Arrange
            var repo = new CartRepository(_context);
            int customerId = 10;
            string optionsJson = "{\"Bo nho trong\":\"256GB\",\"Mau sac\":\"Titan Xanh\"}";

            // Act: Thêm lần 1
            await repo.AddToCartAsync(customerId, productId: 1, quantity: 1, selectedOptionsJson: optionsJson);
            // Act: Thêm lần 2 – cùng SP, cùng cấu hình
            await repo.AddToCartAsync(customerId, productId: 1, quantity: 1, selectedOptionsJson: optionsJson);

            // Assert: Chỉ có 1 dòng, số lượng = 2
            var cart = await repo.GetCartByCustomerIdAsync(customerId);
            Assert.Multiple(() =>
            {
                Assert.That(cart.Details.Count, Is.EqualTo(1), "Phải gộp thành 1 dòng");
                Assert.That(cart.Details.First().Quantity, Is.EqualTo(2), "Số lượng phải là 2");
            });
        }

        // ─────────────────────────────────────────────────────────────────────────
        // TEST 2: Tách dòng – Cùng ProductId + Khác cấu hình → 2 dòng riêng biệt
        // ─────────────────────────────────────────────────────────────────────────
        [Test]
        public async Task AddToCart_TachDong_KhiCungProductId_NhungKhacOptionsJson()
        {
            // Arrange
            var repo = new CartRepository(_context);
            int customerId = 11;
            string optionXanh = "{\"Mau sac\":\"Titan Xanh\",\"Bo nho trong\":\"256GB\"}";
            string optionDen  = "{\"Mau sac\":\"Titan Den\",\"Bo nho trong\":\"256GB\"}";

            // Act: Thêm màu Xanh
            await repo.AddToCartAsync(customerId, productId: 1, quantity: 1, selectedOptionsJson: optionXanh);
            // Act: Thêm màu Đen – cùng SP nhưng khác màu
            await repo.AddToCartAsync(customerId, productId: 1, quantity: 1, selectedOptionsJson: optionDen);

            // Assert: Phải có 2 dòng riêng biệt
            var cart = await repo.GetCartByCustomerIdAsync(customerId);
            Assert.That(cart.Details.Count, Is.EqualTo(2), "Phải tách thành 2 dòng riêng vì khác màu sắc");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // TEST 3: Backward Compatible – Không có options → gộp như cũ theo ProductId
        // ─────────────────────────────────────────────────────────────────────────
        [Test]
        public async Task AddToCart_KhongCoOption_GopNhuCu_TheoProductId()
        {
            // Arrange
            var repo = new CartRepository(_context);
            int customerId = 12;

            // Act: Thêm 2 lần không có options
            await repo.AddToCartAsync(customerId, productId: 1, quantity: 1);
            await repo.AddToCartAsync(customerId, productId: 1, quantity: 2);

            // Assert: Gộp thành 1 dòng, SL = 3
            var cart = await repo.GetCartByCustomerIdAsync(customerId);
            Assert.Multiple(() =>
            {
                Assert.That(cart.Details.Count, Is.EqualTo(1), "Không có options → gộp thành 1 dòng");
                Assert.That(cart.Details.First().Quantity, Is.EqualTo(3), "Số lượng phải là 3");
            });
        }

        // ─────────────────────────────────────────────────────────────────────────
        // TEST 4: Cross-selling – Trả về đúng sản phẩm gợi ý với giá ưu đãi
        // ─────────────────────────────────────────────────────────────────────────
        [Test]
        public async Task GetCrossSellSuggestions_TraVeDungSanPhamGoiY_VoiGiaUuDai()
        {
            // Arrange
            var repo = new CartRepository(_context);
            var productIdsInCart = new List<int> { 1 }; // Giỏ có iPhone 15 Pro

            // Act
            var suggestions = await repo.GetCrossSellSuggestionsAsync(productIdsInCart);

            // Assert: Phải gợi ý Ốp lưng với giá 99.000đ
            Assert.Multiple(() =>
            {
                Assert.That(suggestions.Count, Is.GreaterThan(0), "Phải có ít nhất 1 gợi ý cross-sell");
                Assert.That(suggestions[0].SuggestedProductId, Is.EqualTo(2), "Sản phẩm gợi ý phải là Ốp lưng (ProductId=2)");
                Assert.That(suggestions[0].DiscountedPrice, Is.EqualTo(99000m), "Giá ưu đãi phải là 99.000đ");
            });
        }

        // ─────────────────────────────────────────────────────────────────────────
        // TEST 5: Cross-selling – Không gợi ý sản phẩm đã có trong giỏ
        // ─────────────────────────────────────────────────────────────────────────
        [Test]
        public async Task GetCrossSellSuggestions_KhongGoiY_SanPhamDaCoTrongGio()
        {
            // Arrange
            var repo = new CartRepository(_context);
            // Giỏ có cả iPhone 15 Pro VÀ Ốp lưng → không nên gợi ý Ốp lưng nữa
            var productIdsInCart = new List<int> { 1, 2 };

            // Act
            var suggestions = await repo.GetCrossSellSuggestionsAsync(productIdsInCart);

            // Assert: Không có gợi ý nào (Ốp lưng đã trong giỏ)
            Assert.That(suggestions.Count, Is.EqualTo(0), "Không được gợi ý sản phẩm đã có trong giỏ");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // TEST 6: JSON chuẩn hóa – Thứ tự key khác nhau vẫn được coi là cùng cấu hình
        // ─────────────────────────────────────────────────────────────────────────
        [Test]
        public async Task AddToCart_GopDong_DuKeyJSON_CoThuTuKhacNhau()
        {
            // Arrange
            var repo = new CartRepository(_context);
            int customerId = 13;
            // Cùng cấu hình nhưng thứ tự key khác nhau
            string options1 = "{\"Mau sac\":\"Titan Xanh\",\"Bo nho trong\":\"256GB\"}";
            string options2 = "{\"Bo nho trong\":\"256GB\",\"Mau sac\":\"Titan Xanh\"}"; // Key đảo

            // Act
            await repo.AddToCartAsync(customerId, productId: 1, quantity: 1, selectedOptionsJson: options1);
            await repo.AddToCartAsync(customerId, productId: 1, quantity: 1, selectedOptionsJson: options2);

            // Assert: Phải coi là cùng cấu hình → gộp
            var cart = await repo.GetCartByCustomerIdAsync(customerId);
            Assert.Multiple(() =>
            {
                Assert.That(cart.Details.Count, Is.EqualTo(1), "Thứ tự key JSON khác nhau phải được coi là cùng cấu hình");
                Assert.That(cart.Details.First().Quantity, Is.EqualTo(2));
            });
        }
    }
}
