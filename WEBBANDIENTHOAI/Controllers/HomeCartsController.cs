using Microsoft.AspNetCore.Mvc;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Repository;

namespace WEBBANDIENTHOAI.Controllers
{
    public class HomeCartsController : Controller
    {
        private readonly ICartRepository _cartRepository;

        public HomeCartsController(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<IActionResult> Index()
        {
            // Tạm thời dùng customerId = 1, sau này sẽ thay bằng customerId từ login
            var customerId = 1;

            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId);
            var viewModel = MapToViewModel(cart);

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                var customerId = 1; // Tạm thời
                await _cartRepository.AddToCartAsync(customerId, productId, quantity);

                TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng!";
                return Json(new { success = true, message = "Đã thêm vào giỏ hàng" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartDetailId, int quantity)
        {
            try
            {
                await _cartRepository.UpdateCartItemAsync(cartDetailId, quantity);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(int cartDetailId)
        {
            try
            {
                await _cartRepository.RemoveFromCartAsync(cartDetailId);
                TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            try
            {
                var customerId = 1;
                await _cartRepository.ClearCartAsync(customerId);
                TempData["SuccessMessage"] = "Đã xóa tất cả sản phẩm trong giỏ hàng!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        private CartViewModel MapToViewModel(Cart cart)
        {
            return new CartViewModel
            {
                CartId = cart.CartId,
                Items = cart.Details.Select(d => new CartItemViewModel
                {
                    CartDetailId = d.CartDetailId,
                    ProductId = d.ProductId,
                    ProductName = d.Product.Name,
                    ImageUrl = d.Product.PrimaryImage != null
                        ? $"/images/products/{d.ProductId}.png" // Hoặc URL từ database
                        : "/images/default-product.png",
                    Color = d.Product.Color ?? "Đen",
                    Size = d.Product.Size ?? "Mặc định",
                    UnitPrice = d.UnitPrice,
                    Quantity = d.Quantity
                }).ToList(),
                ShippingFee = cart.Details.Any() ? 0 : 0, // Miễn phí vận chuyển
                Discount = 0
            };
        }
    }
}