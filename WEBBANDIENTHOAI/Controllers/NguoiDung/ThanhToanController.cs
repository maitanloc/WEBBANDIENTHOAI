using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text.Json;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Services;
using WEBBANDIENTHOAI.Services.VNPay;
using WEBBANDIENTHOAI.ViewModels;

namespace WEBBANDIENTHOAI.Controllers.NguoiDung
{
    public class ThanhToanController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IVnPayService _vnPayService;
        private readonly IPromotionService _promotionSvc;
        private const string CheckoutSessionKey = "CheckoutData";

        public ThanhToanController(AppDbContext context, IVnPayService vnPayService, IPromotionService promotionSvc)
        {
            _context = context;
            _vnPayService = vnPayService;
            _promotionSvc = promotionSvc;
        }

        public IActionResult Index(string selectedProductIds)
        {
            var customerId = HttpContext.Session.GetString("UserId");
            var role = HttpContext.Session.GetString("RoleName");

            if (string.IsNullOrEmpty(customerId) || role != "Customer")
            {
                return RedirectToAction("Login", "Account");
            }

            var productIds = selectedProductIds?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList() ?? new List<int>();

            if (!productIds.Any())
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ít nhất một sản phẩm để thanh toán";
                return RedirectToAction("Index", "HomeCarts");
            }

            var customer = _context.Customers.Find(int.Parse(customerId));
            if (customer == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = _context.Carts
                .Include(c => c.Details)
                .ThenInclude(cd => cd.Product)
                .FirstOrDefault(c => c.CustomerId == int.Parse(customerId));

            if (cart == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy giỏ hàng";
                return RedirectToAction("Index", "HomeCarts");
            }

            var selectedItems = cart.Details
                .Where(cd => productIds.Contains(cd.ProductId))
                .Select(cd =>
                {
                    // Tạo chuỗi hiển thị tùy chọn cấu hình
                    var selectedOptionsDisplay = "";
                    if (!string.IsNullOrEmpty(cd.SelectedOptions))
                    {
                        try
                        {
                            var optionsDict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(cd.SelectedOptions);
                            if (optionsDict != null)
                                selectedOptionsDisplay = string.Join(" | ", optionsDict.Select(kv => $"{kv.Key}: {kv.Value}"));
                        }
                        catch { /* ignore parse errors */ }
                    }

                    return new WEBBANDIENTHOAI.ViewModels.CartItemViewModel
                    {
                        CartDetailId = cd.CartDetailId,
                        ProductId = cd.ProductId,
                        ProductName = cd.Product.Name ?? "Sản phẩm không tồn tại",
                        ProductImage = cd.Product.ImageId.HasValue ? $"/Image/ProductImage/{cd.Product.ImageId.Value}" : "/Images/default.jpg",
                        Price = cd.UnitPrice,
                        OptionsPrice = cd.OptionsPrice,  // Giữ đúng giá theo cấu hình
                        Quantity = cd.Quantity,
                        SelectedOptions = cd.SelectedOptions,
                        SelectedOptionsDisplay = selectedOptionsDisplay
                    };
                })
                .ToList();

            var viewModel = new CheckoutViewModel
            {
                Customer = customer,
                SelectedItems = selectedItems,
                TotalAmount = selectedItems.Sum(item => (item.Price + item.OptionsPrice) * item.Quantity),
                Phone = customer.Phone ?? "",
                FullName = customer.FullName,
                Address = customer.Address ?? "",
                Email = customer.Email
            };

            ViewBag.LoyaltyPoints = customer.LoyaltyPoints;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Preview(CheckoutViewModel model)
        {
            var customerId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(customerId))
            {
                return RedirectToAction("Login", "Account");
            }

            // Validate required fields
            if (string.IsNullOrEmpty(model.FullName) || string.IsNullOrEmpty(model.Phone) ||
                string.IsNullOrEmpty(model.Address) || string.IsNullOrEmpty(model.Email))
            {
                TempData["ErrorMessage"] = "Vui lòng điền đầy đủ thông tin bắt buộc";
                return View("Index", model);
            }

            // Lấy lại thông tin sản phẩm từ database để đảm bảo tính nhất quán
            var productIds = model.SelectedItems.Select(item => item.ProductId).ToList();
            var products = _context.Products
                .Where(p => productIds.Contains(p.ProductId))
                .ToDictionary(p => p.ProductId, p => new { p.Name, p.ImageId });

            // Cập nhật thông tin sản phẩm
            foreach (var item in model.SelectedItems)
            {
                if (products.ContainsKey(item.ProductId))
                {
                    var product = products[item.ProductId];
                    item.ProductName = product.Name;
                    item.ProductImage = product.ImageId.HasValue
                        ? $"/Image/ProductImage/{product.ImageId.Value}"
                        : "/Images/default.jpg";
                }
            }

            // Lấy thông tin voucher và điểm từ Session gán vào model để check lại ở Preview
            model.AppliedVoucherCode = HttpContext.Session.GetString("AppliedVoucherCode");
            
            if (decimal.TryParse(HttpContext.Session.GetString("AppliedVoucherDiscount"), out decimal voucherDisc))
                model.VoucherDiscount = voucherDisc;

            model.PointsUsed = HttpContext.Session.GetInt32("AppliedPoints") ?? 0;
            
            if (decimal.TryParse(HttpContext.Session.GetString("AppliedPointsMoney"), out decimal pointsDisc))
                model.PointsDiscount = pointsDisc;

            // Lưu tạm thông tin thanh toán vào session
            HttpContext.Session.SetString(CheckoutSessionKey, JsonSerializer.Serialize(model));

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableVouchers()
        {
            var customerId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(customerId)) return Json(new List<object>());

            var userVouchers = await _context.UserVouchers
                .Include(uv => uv.Voucher)
                .Where(uv => uv.CustomerId == int.Parse(customerId) && !uv.IsUsed 
                    && uv.Voucher.IsActive && uv.Voucher.EndDate >= DateTime.UtcNow
                    && uv.Voucher.UsedCount < uv.Voucher.Quantity)
                .Select(uv => new {
                    code = uv.Voucher.Code,
                    description = uv.Voucher.Description,
                    minOrderValue = uv.Voucher.MinOrderValue,
                    discountType = uv.Voucher.DiscountType
                }).ToListAsync();

            return Json(userVouchers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyVoucher([FromForm] string voucherCode, [FromForm] decimal totalAmount)
        {
            var customerId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(customerId)) return Json(new { success = false, message = "Vui lòng đăng nhập." });

            var result = await _promotionSvc.ValidateVoucherAsync(voucherCode, totalAmount, int.Parse(customerId));
            if (result.IsValid && result.Voucher != null)
            {
                HttpContext.Session.SetString("AppliedVoucherCode", voucherCode);
                HttpContext.Session.SetInt32("AppliedVoucherId", result.Voucher.VoucherId);
                HttpContext.Session.SetString("AppliedVoucherDiscount", result.DiscountAmount.ToString());
                return Json(new { success = true, discountAmount = result.DiscountAmount, message = "Áp dụng voucher thành công!" });
            }
            return Json(new { success = false, message = result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UsePoints([FromForm] int points)
        {
            var customerId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(customerId)) return Json(new { success = false, message = "Vui lòng đăng nhập." });

            if (points < 10 && points != 0) return Json(new { success = false, message = "Cần sử dụng tối thiểu 10 điểm." });

            var customer = _context.Customers.Find(int.Parse(customerId));
            if (customer == null || customer.LoyaltyPoints < points)
            {
                return Json(new { success = false, message = "Điểm F-Point không đủ." });
            }

            decimal discount = points * 1000m; // 1 F-Point = 1.000đ
            if (points == 0) discount = 0m;
            
            HttpContext.Session.SetInt32("AppliedPoints", points);
            HttpContext.Session.SetString("AppliedPointsMoney", discount.ToString());
            
            return Json(new { success = true, discountAmount = discount, message = points == 0 ? "Bỏ dùng điểm." : $"Sử dụng {points} F-Point thành công!" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProcessOrder()
        {
            var customerId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(customerId))
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập lại." });
            }

            // Lấy thông tin thanh toán từ session
            var modelJson = HttpContext.Session.GetString(CheckoutSessionKey);
            if (string.IsNullOrEmpty(modelJson))
            {
                return Json(new { success = false, message = "Thông tin thanh toán không tồn tại hoặc đã hết hạn." });
            }

            var model = JsonSerializer.Deserialize<CheckoutViewModel>(modelJson);

            try
            {
                int orderId = 0; // Khai báo orderId ngoài để sử dụng sau

                var strategy = _context.Database.CreateExecutionStrategy(); // Tạo execution strategy

                IActionResult result = null; 

                strategy.Execute(() => // Bọc toàn bộ transaction vào strategy để hỗ trợ retry
                {
                    using var transaction = _context.Database.BeginTransaction(); // Bắt đầu transaction

                    try
                    {
                        // 0. KIỂM TRA TỒN KHO (Overselling Prevention)
                        foreach (var item in model.SelectedItems)
                        {
                            var totalStock = _context.Inventories
                                .Where(i => i.ProductId == item.ProductId)
                                .Sum(i => i.CurrentQuantity);

                            if (totalStock < item.Quantity)
                            {
                                // Nếu thiếu hàng, gán result và return để thoát lambda
                                result = Json(new { success = false, message = $"Sản phẩm '{item.ProductName}' chỉ còn {totalStock} sản phẩm. Vui lòng cập nhật giỏ hàng." });
                                return;
                            }
                        }

                        // ===== PROMOTION & LOYALTY (Re-validate before saving order) =====
                        int? appliedVoucherId = HttpContext.Session.GetInt32("AppliedVoucherId");
                        string? appliedVoucherCode = HttpContext.Session.GetString("AppliedVoucherCode");
                        decimal currentDiscount = 0m;
                        int pointsUsed = HttpContext.Session.GetInt32("AppliedPoints") ?? 0;

                        if (!string.IsNullOrEmpty(appliedVoucherCode) && appliedVoucherId.HasValue)
                        {
                            // ValidateVoucherAsync is async, wait for it
                            var vResTask = _promotionSvc.ValidateVoucherAsync(appliedVoucherCode, model.TotalAmount, int.Parse(customerId));
                            vResTask.Wait(); // Blocking call trong Strategy có thể gây chú ý, tốt nhất nên validate ở ngoài ExecutionStrategy, 
                                             // nhưng ở đây ExecutionStrategy cho EFCore hỗ trợ async rất tốt. 
                                             // Sẽ sửa chỗ này thành Validate đồng bộ hoặc bọc strategy async ở hàm bao ngoài nếu lỗi.
                                             // Vì lambda trong Execute hiện tại là KHÔNG ASYNC: `strategy.Execute(() => ...)`
                            var vRes = vResTask.Result;

                            if (!vRes.IsValid) {
                                result = Json(new { success = false, message = $"Voucher không hợp lệ: {vRes.Message}. Vui lòng tải lại trang." });
                                return;
                            }
                            currentDiscount = vRes.DiscountAmount;
                        }

                        // Guard điểm
                        if (pointsUsed > 0)
                        {
                            var cust = _context.Customers.Find(int.Parse(customerId));
                            if (cust == null || cust.LoyaltyPoints < pointsUsed) {
                                result = Json(new { success = false, message = "Điểm F-Point không đủ hoặc không hợp lệ." });
                                return;
                            }
                        }

                        // Tính toán giá sau cùng (Final Amount)
                        decimal pointsDiscount = pointsUsed * 1000m;
                        decimal finalTotal = Math.Max(0, model.TotalAmount - currentDiscount - pointsDiscount);

                        // 1. Tạo đơn hàng mới
                        var order = new Order
                        {
                            CustomerId = int.Parse(customerId),
                            OrderDate = DateTime.UtcNow,
                            Total = finalTotal, // Lưu giá ĐÃ GIẢM vào DB để gửi qua VNPay
                            VoucherId = appliedVoucherId,
                            DiscountAmount = currentDiscount,
                            PointsUsed = pointsUsed,
                            StatusId = 1,
                            ShippingAddress = model.Address,
                            CreatedByUserId = null,
                            PaymentMethod = model.PaymentMethod,
                            Notes = model.Notes ?? string.Empty, // Set to empty string if null to avoid null issues
                            Latitude = model.Latitude,
                            Longitude = model.Longitude
                        };

                        _context.Orders.Add(order);
                        _context.SaveChanges();

                        orderId = order.OrderId; // Gán orderId sau khi SaveChanges()

                        // 2. Tạo chi tiết đơn hàng
                        foreach (var item in model.SelectedItems)
                        {
                            var orderDetail = new OrderDetail
                            {
                                OrderId = order.OrderId,
                                ProductId = item.ProductId,
                                Quantity = item.Quantity,
                                UnitPrice = item.Price,
                                ProductName = item.ProductName
                            };
                            _context.OrderDetails.Add(orderDetail);
                        }

                        // 3. Xóa các sản phẩm đã thanh toán khỏi giỏ hàng
                        var cart = _context.Carts
                            .Include(c => c.Details)
                            .FirstOrDefault(c => c.CustomerId == int.Parse(customerId));

                        if (cart != null)
                        {
                            var selectedCartDetails = cart.Details
                                .Where(cd => model.SelectedItems.Select(si => si.ProductId).Contains(cd.ProductId))
                                .ToList();

                            foreach (var cartDetail in selectedCartDetails)
                            {
                                _context.CartDetails.Remove(cartDetail);
                            }
                        }

                        _context.SaveChanges();

                        if (model.PaymentMethod == "BankTransfer")
                        {
                            // KHÔNG commit voucher và điểm ngay cho VNPay, chờ Callback
                            var paymentModel = new PaymentInformationModel
                            {
                                Amount = (double)order.Total,
                                Name = model.FullName,
                                OrderDescription = $"DH{order.OrderId}",
                                OrderType = "other",
                                OrderId = order.OrderId
                            };
                            var paymentUrl = _vnPayService.CreatePaymentUrl(paymentModel, HttpContext);
                            
                            // Giữ lại Session vì Callback cần? Thực ra Callback không cần vì ID voucher lưu vào order rồi
                            transaction.Commit();
                            result = Json(new { success = true, message = "Chuyển hướng thanh toán VNPay...", redirectUrl = paymentUrl });
                        }
                        else 
                        {
                            // ===== COMMIT VOUCHER & POINTS CHO COD =====
                            if (appliedVoucherId.HasValue)
                            {
                                var commitTask = _promotionSvc.CommitVoucherUsageAsync(appliedVoucherId.Value, int.Parse(customerId), order.OrderId);
                                commitTask.Wait();
                            }
                            if (pointsUsed > 0)
                            {
                                var usePointsTask = _promotionSvc.UsePointsAsync(int.Parse(customerId), pointsUsed);
                                usePointsTask.Wait();
                            }

                            transaction.Commit(); // Commit transaction

                            // Xóa session sau khi xử lý thành công
                            HttpContext.Session.Remove(CheckoutSessionKey);
                            HttpContext.Session.Remove("AppliedVoucherCode");
                            HttpContext.Session.Remove("AppliedVoucherId");
                            HttpContext.Session.Remove("AppliedVoucherDiscount");
                            HttpContext.Session.Remove("AppliedPoints");
                            HttpContext.Session.Remove("AppliedPointsMoney");
                            
                            result = Json(new { success = true, message = "Đặt hàng thành công!", redirectUrl = Url.Action("OrderSuccess", new { orderId }) });
                        }
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw; // Re-throw để catch ở ngoài xử lý logging
                    }
                });

                if (result != null)
                {
                    return result;
                }
                
                // Should not happen if strategy executes successfully or catches exception
                return Json(new { success = false, message = "Lỗi không xác định khi xử lý đơn hàng." });


            }
            catch (Exception ex)
            {
                // Không cần rollback thủ công vì strategy sẽ xử lý
                Console.WriteLine($"Error in ProcessOrder: {ex.Message} - StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = $"Có lỗi xảy ra khi đặt hàng: {ex.Message}. Vui lòng thử lại." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder()
        {
            // Xóa dữ liệu thanh toán khỏi session khi hủy
            HttpContext.Session.Remove(CheckoutSessionKey);

            TempData["InfoMessage"] = "Đã hủy đơn hàng. Bạn có thể chỉnh sửa thông tin và thử lại.";
            return RedirectToAction("Index", "HomeCarts");
        }

        public IActionResult OrderSuccess(int orderId)
        {
            // Lấy thông tin đơn hàng đầy đủ
            var order = _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Include(o => o.Voucher) // Include thêm Voucher
                .FirstOrDefault(o => o.OrderId == orderId);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng";
                return RedirectToAction("Index", "HomeCarts");
            }

            // Tạo view model cho bill
            var billViewModel = new OrderBillViewModel
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                CustomerName = order.Customer?.FullName ?? string.Empty,
                CustomerEmail = order.Customer?.Email ?? string.Empty,
                CustomerPhone = order.Customer?.Phone ?? string.Empty,
                ShippingAddress = order.ShippingAddress ?? string.Empty,
                TotalAmount = order.OrderDetails != null ? order.OrderDetails.Sum(od => od.Quantity * od.UnitPrice) : order.Total + order.DiscountAmount,
                VoucherDiscount = order.DiscountAmount,
                PointsUsed = order.PointsUsed,
                PointsDiscount = order.PointsUsed > 0 ? order.PointsUsed * 1000 : 0,
                AppliedVoucherCode = order.Voucher?.Code,
                PaymentMethod = order.PaymentMethod ?? string.Empty,
                Status = order.OrderStatus?.StatusName ?? "Pending",
                OrderItems = order.OrderDetails.Select(od => new OrderItemViewModel
                {
                    ProductName = od.Product?.Name ?? "Unknown",
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,
                    TotalPrice = od.Quantity * od.UnitPrice
                }).ToList()
            };

            // Set ViewBag for Layout
            ViewBag.IsLoggedIn = true;
            ViewBag.CustomerName = order.Customer?.FullName;
            ViewBag.CustomerEmail = order.Customer?.Email;
            ViewBag.CustomerPhone = order.Customer?.Phone;

            return View(billViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            try
            {
                var response = _vnPayService.PaymentExecute(Request.Query);

                if (response.Success)
                {
                    // Cập nhật trạng thái đơn hàng thành đã thanh toán
                    var orderId = long.Parse(response.OrderId);
                    var order = await _context.Orders.FindAsync((int)orderId);

                    if (order != null)
                    {
                        // Kiểm tra nếu đơn hàng chưa thanh toán thì mới cập nhật
                        if (order.StatusId != 2)
                        {
                            order.StatusId = 2; // Đã thanh toán
                            order.PaymentMethod = "VNPay";
                            
                            // Commit voucher and points
                            if (order.VoucherId.HasValue)
                            {
                                await _promotionSvc.CommitVoucherUsageAsync(order.VoucherId.Value, order.CustomerId, order.OrderId);
                            }
                            if (order.PointsUsed > 0)
                            {
                                await _promotionSvc.UsePointsAsync(order.CustomerId, order.PointsUsed);
                            }

                            await _context.SaveChangesAsync();
                        }

                        TempData["SuccessMessage"] = $"Thanh toán thành công đơn hàng #{orderId}";
                        return RedirectToAction("OrderSuccess", new { orderId = order.OrderId });
                    }
                }

                TempData["ErrorMessage"] = "Thanh toán thất bại hoặc có lỗi xác thực chữ ký. Vui lòng thử lại.";
                return RedirectToAction("Index", "HomeCarts");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PaymentCallbackVnpay: {ex.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xử lý thanh toán.";
                return RedirectToAction("Index", "HomeCarts");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PaymentNotify()
        {
            try
            {
                var response = _vnPayService.PaymentExecute(Request.Query);

                if (response.Success)
                {
                    var orderId = long.Parse(response.OrderId);
                    var order = await _context.Orders.FindAsync((int)orderId);

                    if (order != null)
                    {
                        // Kiểm tra số tiền thanh toán có khớp không
                        // Note: vnp_Amount is multiplied by 100 in request, so potentially verify it here if available in response
                        // For now we assume signature is enough validation for integrity

                        if (order.StatusId != 2) // Nếu chưa thanh toán
                        {
                            order.StatusId = 2; // Đã thanh toán
                            order.PaymentMethod = "VNPay";
                            
                            // Commit voucher and points
                            if (order.VoucherId.HasValue)
                            {
                                await _promotionSvc.CommitVoucherUsageAsync(order.VoucherId.Value, order.CustomerId, order.OrderId);
                            }
                            if (order.PointsUsed > 0)
                            {
                                await _promotionSvc.UsePointsAsync(order.CustomerId, order.PointsUsed);
                            }

                            await _context.SaveChangesAsync();
                        }
                        
                        return Json(new { RspCode = "00", Message = "Confirm Success" });
                    }
                    else
                    {
                        return Json(new { RspCode = "01", Message = "Order not found" });
                    }
                }
                else
                {
                     // Invalid signature
                     return Json(new { RspCode = "97", Message = "Invalid Checksum" });
                }
            }
            catch (Exception ex)
            {
               Console.WriteLine($"Error in PaymentNotify: {ex.Message}");
               return Json(new { RspCode = "99", Message = "Unknow error" });
            }
        }

        

        // Method CreateVnpayPayment removed, logic moved to ProcessOrder

            }

        }