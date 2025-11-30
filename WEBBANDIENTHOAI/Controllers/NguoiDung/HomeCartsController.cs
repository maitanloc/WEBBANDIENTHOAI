using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Repository.NguoiDung;
using WEBBANDIENTHOAI.ViewModels;

namespace WEBBANDIENTHOAI.Controllers.NguoiDung
{
    public class HomeCartsController : Controller
    {
        private readonly ICartRepository _cartRepository;
        private readonly AppDbContext _context;

        public HomeCartsController(ICartRepository cartRepository, AppDbContext context)
        {
            _cartRepository = cartRepository;
            _context = context;
        }

        private int? GetCurrentCustomerId()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("RoleName");

            if (!string.IsNullOrEmpty(userIdString) && role == "Customer")
            {
                if (int.TryParse(userIdString, out int customerId))
                {
                    return customerId;
                }
            }
            return null;
        }

        public async Task<IActionResult> Index()
        {
            var customerId = GetCurrentCustomerId();
            if (customerId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId.Value);
            var viewModel = await MapToCartIndexViewModel(cart);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                var customerId = GetCurrentCustomerId();
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

        public async Task<IActionResult> BuyNow(int productId, int quantity = 1)
        {
            var customerId = GetCurrentCustomerId();
            if (customerId == null)
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang đăng nhập và lưu lại trang định quay về
                return RedirectToAction("LoginRegister", "Account", new { returnUrl = Url.Action("Productdetails", "HomeUser", new { id = productId }) });
            }

            try
            {
                await _cartRepository.AddToCartAsync(customerId.Value, productId, quantity);

                // Chuyển hướng thẳng đến trang thanh toán với ID sản phẩm vừa thêm
                return RedirectToAction("Index", "ThanhToan", new { selectedProductIds = productId.ToString() });
            }
            catch (Exception)
            {
                // Xử lý lỗi nếu có, ví dụ: hiển thị thông báo lỗi và quay lại trang chi tiết sản phẩm
                TempData["ErrorMessage"] = "Không thể thêm sản phẩm vào giỏ hàng. Vui lòng thử lại.";
                return RedirectToAction("Productdetails", "HomeUser", new { id = productId });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            try
            {
                var customerId = GetCurrentCustomerId();
                if (customerId == null)
                {
                    return Json(new { success = false, message = "Phiên đăng nhập hết hạn." });
                }

                await _cartRepository.UpdateCartItemAsync(customerId.Value, productId, quantity);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            try
            {
                var customerId = GetCurrentCustomerId();
                if (customerId == null)
                    return RedirectToAction("Login", "Account");

                await _cartRepository.RemoveFromCartAsync(customerId.Value, productId);
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

        private async Task<CartIndexViewModel> MapToCartIndexViewModel(Cart cart)
        {
            var viewModel = new CartIndexViewModel();

            if (cart?.Details != null)
            {
                foreach (var item in cart.Details)
                {
                    // Lấy thông tin sản phẩm
                    var product = await _context.Products
                        .Include(p => p.PrimaryImage)
                        .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);

                    if (product != null)
                    {
                        // Lấy ảnh chính của sản phẩm
                        string imageUrl = product.PrimaryImage != null
                            ? Url.Action("GetProductImage", "HomeUser", new { imageId = product.PrimaryImage.ImageId })
                            : "/images/default-product.png";

                        // SỬA DÒNG NÀY - THÊM ĐẦY ĐỦ NAMESPACE
                        viewModel.CartItems.Add(new ViewModels.CartItemViewModel
                        {
                            CartDetailId = item.CartDetailId,
                            ProductId = item.ProductId,
                            ProductName = product.Name,
                            ProductImage = imageUrl,
                            Price = item.UnitPrice,
                            Quantity = item.Quantity
                        });
                    }
                }
            }

            return viewModel;
        }
    }
}