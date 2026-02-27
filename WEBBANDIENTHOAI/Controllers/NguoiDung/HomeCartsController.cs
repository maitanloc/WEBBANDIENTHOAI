using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Repository.NguoiDung;
using WEBBANDIENTHOAI.Repository.TaiKhoan;
using WEBBANDIENTHOAI.ViewModels;

namespace WEBBANDIENTHOAI.Controllers.NguoiDung
{
    public class HomeCartsController : Controller
    {
        private readonly ICartRepository _cartRepository;
        private readonly AppDbContext _context;
        private readonly ICustomerRepository _customerRepository;

        public HomeCartsController(ICartRepository cartRepository, AppDbContext context, ICustomerRepository customerRepository)
        {
            _cartRepository = cartRepository;
            _context = context;
            _customerRepository = customerRepository;
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

            var customer = await _customerRepository.GetCustomerByIdAsync(customerId.Value);
            if (customer != null)
            {
                ViewBag.CustomerName = customer.FullName;
                ViewBag.CustomerEmail = customer.Email;
                ViewBag.CustomerPhone = customer.Phone;
                ViewBag.IsLoggedIn = true;
            }
            else
            {
                ViewBag.IsLoggedIn = false;
            }

            var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId.Value);
            var viewModel = await MapToCartIndexViewModel(cart);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1, string? selectedOptions = null)
        {
            try
            {
                var customerId = GetCurrentCustomerId();
                if (customerId == null)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để mua hàng!", requireLogin = true });
                }

                // Check stock
                var product = await _context.Products.Include(p => p.Inventory).FirstOrDefaultAsync(p => p.ProductId == productId);
                if (product == null) return Json(new { success = false, message = "Sản phẩm không tồn tại!" });

                var currentStock = product.Inventory?.Sum(i => i.CurrentQuantity) ?? 0;

                // Get current cart quantity for this product+option combo
                var cart = await _cartRepository.GetCartByCustomerIdAsync(customerId.Value);
                var existingItem = cart?.Details?.FirstOrDefault(d => d.ProductId == productId);
                var currentCartQuantity = existingItem?.Quantity ?? 0;

                if (currentCartQuantity + quantity > currentStock)
                {
                    return Json(new { success = false, message = $"Số lượng sản phẩm không đủ. Tối đa: {currentStock}" });
                }

                await _cartRepository.AddToCartAsync(customerId.Value, productId, quantity, selectedOptions);
                return Json(new { success = true, message = "Đã thêm vào giỏ hàng" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

        public async Task<IActionResult> BuyNow(int productId, int quantity = 1, string? selectedOptions = null)
        {
            var customerId = GetCurrentCustomerId();
            if (customerId == null)
            {
                return RedirectToAction("LoginRegister", "Account", new { returnUrl = Url.Action("Productdetails", "HomeUser", new { id = productId }) });
            }

            try
            {
                // Check stock
                var product = await _context.Products.Include(p => p.Inventory).FirstOrDefaultAsync(p => p.ProductId == productId);
                if (product == null)
                {
                    TempData["ErrorMessage"] = "Sản phẩm không tồn tại!";
                    return RedirectToAction("Productdetails", "HomeUser", new { id = productId });
                }

                var currentStock = product.Inventory?.Sum(i => i.CurrentQuantity) ?? 0;
                if (quantity > currentStock)
                {
                    TempData["ErrorMessage"] = $"Số lượng sản phẩm không đủ. Tối đa: {currentStock}";
                    return RedirectToAction("Productdetails", "HomeUser", new { id = productId });
                }

                await _cartRepository.AddToCartAsync(customerId.Value, productId, quantity, selectedOptions);
                return RedirectToAction("Index", "ThanhToan", new { selectedProductIds = productId.ToString() });
            }
            catch (Exception)
            {
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

                // Check stock
                var product = await _context.Products.Include(p => p.Inventory).FirstOrDefaultAsync(p => p.ProductId == productId);
                if (product == null) return Json(new { success = false, message = "Sản phẩm không tồn tại!" });

                var currentStock = product.Inventory?.Sum(i => i.CurrentQuantity) ?? 0;

                if (quantity > currentStock)
                {
                    return Json(new { success = false, message = $"Số lượng sản phẩm không đủ. Tối đa: {currentStock}" });
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
                    // Lấy thông tin sản phẩm và tồn kho
                    var product = await _context.Products
                        .Include(p => p.PrimaryImage)
                        .Include(p => p.Inventory) // Thêm dòng này
                        .FirstOrDefaultAsync(p => p.ProductId == item.ProductId);

                    if (product != null)
                    {
                        // Lấy ảnh chính
                        string imageUrl = product.PrimaryImage != null
                            ? Url.Action("GetProductImage", "HomeUser", new { imageId = product.PrimaryImage.ImageId })
                            : "/images/default-product.png";

                        // Tính tổng tồn kho
                        var inventoryQuantity = product.Inventory?.Sum(i => i.CurrentQuantity) ?? 0;

                        // Tạo chuỗi hiển thị tùy chọn: "Màu sắc: Titan Xanh | Bộ nhớ: 512GB"
                        var selectedOptionsDisplay = "";
                        if (!string.IsNullOrEmpty(item.SelectedOptions))
                        {
                            try
                            {
                                var optionsDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(item.SelectedOptions);
                                if (optionsDict != null)
                                    selectedOptionsDisplay = string.Join(" | ", optionsDict.Select(kv => $"{kv.Key}: {kv.Value}"));
                            }
                            catch { /* ignore parse errors */ }
                        }

                        viewModel.CartItems.Add(new ViewModels.CartItemViewModel
                        {
                            CartDetailId = item.CartDetailId,
                            ProductId = item.ProductId,
                            ProductName = product.Name,
                            ProductImage = imageUrl,
                            Price = item.UnitPrice,
                            OptionsPrice = item.OptionsPrice,
                            Quantity = item.Quantity,
                            InventoryQuantity = inventoryQuantity,
                            SelectedOptions = item.SelectedOptions,
                            SelectedOptionsDisplay = selectedOptionsDisplay
                        });
                    }
                }
            }

            return viewModel;
        }
    }
}