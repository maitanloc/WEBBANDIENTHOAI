using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Repository.NguoiDung
{
    public interface ICartRepository
    {
        Task<Cart> GetCartByCustomerIdAsync(int customerId);
        Task AddToCartAsync(int customerId, int productId, int quantity = 1, string? selectedOptionsJson = null);
        Task UpdateCartItemAsync(int customerId, int productId, int quantity);
        Task RemoveFromCartAsync(int customerId, int productId);
        Task ClearCartAsync(int customerId);
        Task<List<CrossSellRule>> GetCrossSellSuggestionsAsync(List<int> productIdsInCart);
    }

    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Cart> GetCartByCustomerIdAsync(int customerId)
        {
            return await _context.Carts
                .Include(c => c.Details)
                    .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId)
                ?? await CreateNewCartAsync(customerId);
        }

        private async Task<Cart> CreateNewCartAsync(int customerId)
        {
            var cart = new Cart
            {
                CustomerId = customerId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        /// <summary>
        /// Chuẩn hóa JSON options: sắp xếp key theo alphabet để
        /// {"Màu":"Xanh","Nhớ":"256"} == {"Nhớ":"256","Màu":"Xanh"}
        /// </summary>
        private static string NormalizeOptionsJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return "";
            try
            {
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                if (dict == null) return "";
                var sorted = new SortedDictionary<string, string>(dict, StringComparer.Ordinal);
                return JsonSerializer.Serialize(sorted);
            }
            catch
            {
                return json ?? "";
            }
        }

        /// <summary>
        /// Tính tổng OptionsPrice dựa trên JSON options đã chọn và dữ liệu ProductOption trong DB.
        /// </summary>
        private async Task<decimal> CalcOptionsPriceAsync(int productId, string? selectedOptionsJson)
        {
            if (string.IsNullOrWhiteSpace(selectedOptionsJson)) return 0m;
            try
            {
                var selected = JsonSerializer.Deserialize<Dictionary<string, string>>(selectedOptionsJson);
                if (selected == null || selected.Count == 0) return 0m;

                decimal total = 0m;
                var productOptions = await _context.ProductOptions
                    .Where(po => po.ProductId == productId && po.IsActive)
                    .ToListAsync();

                foreach (var kv in selected)
                {
                    var match = productOptions.FirstOrDefault(po =>
                        po.GroupName == kv.Key && po.OptionName == kv.Value);
                    if (match != null)
                        total += match.AdditionalPrice;
                }
                return total;
            }
            catch
            {
                return 0m;
            }
        }

        public async Task AddToCartAsync(int customerId, int productId, int quantity = 1, string? selectedOptionsJson = null)
        {
            var cart = await GetCartByCustomerIdAsync(customerId);
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
                throw new Exception("Sản phẩm không tồn tại");

            // Chuẩn hóa JSON để so sánh thứ tự key không quan trọng
            var normalizedOptions = NormalizeOptionsJson(selectedOptionsJson);

            // Tìm dòng có cùng ProductId VÀ cùng cấu hình options
            var existingItem = cart.Details?.FirstOrDefault(d =>
                d.ProductId == productId &&
                NormalizeOptionsJson(d.SelectedOptions) == normalizedOptions);

            if (existingItem != null)
            {
                // Gộp dòng: cùng SP + cùng config → tăng số lượng
                existingItem.Quantity += quantity;
            }
            else
            {
                if (cart.Details == null)
                    cart.Details = new List<CartDetail>();

                var optionsPrice = await CalcOptionsPriceAsync(productId, selectedOptionsJson);

                // Tách dòng mới: khác config → thêm dòng riêng
                cart.Details.Add(new CartDetail
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price,
                    SelectedOptions = string.IsNullOrWhiteSpace(selectedOptionsJson) ? null : normalizedOptions,
                    OptionsPrice = optionsPrice
                });
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCartItemAsync(int customerId, int productId, int quantity)
        {
            var cart = await GetCartByCustomerIdAsync(customerId);
            var cartItem = cart.Details?.FirstOrDefault(d => d.ProductId == productId);

            if (cartItem != null)
            {
                if (quantity <= 0)
                    _context.CartDetails.Remove(cartItem);
                else
                    cartItem.Quantity = quantity;

                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveFromCartAsync(int customerId, int productId)
        {
            var cart = await GetCartByCustomerIdAsync(customerId);
            var cartItem = cart.Details?.FirstOrDefault(d => d.ProductId == productId);

            if (cartItem != null)
            {
                _context.CartDetails.Remove(cartItem);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(int customerId)
        {
            var cart = await GetCartByCustomerIdAsync(customerId);
            if (cart.Details != null && cart.Details.Any())
            {
                _context.CartDetails.RemoveRange(cart.Details);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Lấy danh sách sản phẩm gợi ý bán chéo, loại trừ những SP đã có trong giỏ.
        /// </summary>
        public async Task<List<CrossSellRule>> GetCrossSellSuggestionsAsync(List<int> productIdsInCart)
        {
            return await _context.CrossSellRules
                .Include(r => r.SuggestedProduct)
                    .ThenInclude(p => p!.PrimaryImage)
                .Where(r => r.IsActive
                    && productIdsInCart.Contains(r.TriggerProductId)
                    && !productIdsInCart.Contains(r.SuggestedProductId))
                .ToListAsync();
        }
    }
}