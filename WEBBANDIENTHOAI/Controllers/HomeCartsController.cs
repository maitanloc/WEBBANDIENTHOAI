using Microsoft.AspNetCore.Mvc;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Repository;
using Microsoft.AspNetCore.Http; // Cần để dùng Session

namespace WEBBANDIENTHOAI.Controllers
{
    public class HomeCartsController : Controller
    {
        private readonly ICartRepository _cartRepository;

        public HomeCartsController(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        // --- HÀM MỚI: Lấy ID từ Session do AccountController tạo ra ---
        private int? GetCurrentCustomerId()
        {
            // 1. Lấy chuỗi UserId từ Session (AccountController lưu dạng String)
            var userIdString = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("RoleName");

            // 2. Kiểm tra xem có dữ liệu không và Role có phải là Customer không
            // (Để tránh trường hợp Admin đăng nhập nhưng lại vào giỏ hàng mua đồ - tùy logic dự án)
            if (!string.IsNullOrEmpty(userIdString) && role == "Customer")
            {
                if (int.TryParse(userIdString, out int customerId))
                {
                    return customerId;
                }
            }

            return null; // Chưa đăng nhập hoặc không phải Customer
        }

        public async Task<IActionResult> Index()
        {
            // 1. Lấy ID người dùng hiện tại
            var customerId = GetCurrentCustomerId();

            // 2. Nếu chưa đăng nhập -> Chuyển hướng về trang Login của AccountController
            if (customerId == null)
            {
                // Lưu URL hiện tại để sau khi login thì quay lại (nếu muốn phát triển thêm)
                return RedirectToAction("Login", "Account");
            }

            // 3. Lấy giỏ hàng từ DB
            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId.Value);

            // 4. Map sang ViewModel
            var viewModel = MapToViewModel(cart);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                var customerId = GetCurrentCustomerId();

                // Nếu chưa login mà bấm thêm vào giỏ -> Báo lỗi bắt đăng nhập
                if (customerId == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để mua hàng!", requireLogin = true });
                }

                await _cartRepository.AddToCartAsync(customerId.Value, productId, quantity);

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
                // Kiểm tra login
                if (GetCurrentCustomerId() == null)
                {
                    return Json(new { success = false, message = "Phiên đăng nhập hết hạn." });
                }

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
                if (GetCurrentCustomerId() == null) return RedirectToAction("Login", "Account");

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
                var customerId = GetCurrentCustomerId();
                if (customerId != null)
                {
                    await _cartRepository.ClearCartAsync(customerId.Value);
                    TempData["SuccessMessage"] = "Đã xóa tất cả sản phẩm trong giỏ hàng!";
                }
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
            if (cart == null || cart.Details == null)
            {
                return new CartViewModel
                {
                    Items = new List<CartItemViewModel>(),
                    ShippingFee = 0,
                    Discount = 0
                };
            }

            return new CartViewModel
            {
                CartId = cart.CartId,
                Items = cart.Details.Select(d => new CartItemViewModel
                {
                    CartDetailId = d.CartDetailId,
                    ProductId = d.ProductId,
                    ProductName = d.Product?.Name ?? "Sản phẩm lỗi",
                    // Logic lấy ảnh: Nếu PrimaryImage có Url thì lấy, không thì lấy ảnh mặc định
                    ImageUrl = d.Product?.PrimaryImage != null
    ? $"/Image/ProductImage/{d.Product.PrimaryImage.ImageId}"
    : "/images/default-product.png",
                    Color = d.Product?.Color ?? "Đen",
                    Size = d.Product?.Size ?? "Tiêu chuẩn",
                    UnitPrice = d.UnitPrice,
                    Quantity = d.Quantity
                }).ToList(),
                ShippingFee = 0,
                Discount = 0
            };
        }
    }
}