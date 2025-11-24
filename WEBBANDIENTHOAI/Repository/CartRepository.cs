using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Repository
{
    public interface ICartRepository
    {
        Task<Cart> GetCartByCustomerIdAsync(int customerId);
        Task AddToCartAsync(int customerId, int productId, int quantity = 1);
        Task UpdateCartItemAsync(int customerId, int productId, int quantity);
        Task RemoveFromCartAsync(int customerId, int productId);
        Task ClearCartAsync(int customerId);
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

        public async Task AddToCartAsync(int customerId, int productId, int quantity = 1)
        {
            var cart = await GetCartByCustomerIdAsync(customerId);
            var product = await _context.Products.FindAsync(productId);

            if (product == null)
                throw new Exception("Sản phẩm không tồn tại");

            var existingItem = cart.Details?.FirstOrDefault(d => d.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                if (cart.Details == null)
                    cart.Details = new List<CartDetail>();

                cart.Details.Add(new CartDetail
                {
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price
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
                {
                    _context.CartDetails.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = quantity;
                }
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
    }
}