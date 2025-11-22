using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Repository
{
    public interface ICartRepository
    {
        Task<Cart> GetCartByCustomerIdAsync(int customerId);
        Task AddToCartAsync(int customerId, int productId, int quantity = 1);
        Task UpdateCartItemAsync(int cartDetailId, int quantity);
        Task RemoveFromCartAsync(int cartDetailId);
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
                        .ThenInclude(p => p.PrimaryImage)
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

            if (product == null) return;

            var existingItem = cart.Details.FirstOrDefault(d => d.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
                existingItem.UnitPrice = product.Price;
            }
            else
            {
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

        public async Task UpdateCartItemAsync(int cartDetailId, int quantity)
        {
            var cartDetail = await _context.CartDetails.FindAsync(cartDetailId);
            if (cartDetail != null && quantity > 0)
            {
                cartDetail.Quantity = quantity;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveFromCartAsync(int cartDetailId)
        {
            var cartDetail = await _context.CartDetails.FindAsync(cartDetailId);
            if (cartDetail != null)
            {
                _context.CartDetails.Remove(cartDetail);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearCartAsync(int customerId)
        {
            var cart = await GetCartByCustomerIdAsync(customerId);
            _context.CartDetails.RemoveRange(cart.Details);
            await _context.SaveChangesAsync();
        }
    }
}