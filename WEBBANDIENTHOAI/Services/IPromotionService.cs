// WEBBANDIENTHOAI/Services/IPromotionService.cs
using System.Threading.Tasks;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Services
{
    public class VoucherValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        /// <summary>Số tiền giảm tính được (0 nếu invalid)</summary>
        public decimal DiscountAmount { get; set; } = 0m;
        public Voucher? Voucher { get; set; }
    }

    public interface IPromotionService
    {
        // ---------- Voucher ----------
        /// <summary>Kiểm tra mã voucher có hợp lệ với orderTotal không (server-side, không ghi DB)</summary>
        Task<VoucherValidationResult> ValidateVoucherAsync(string code, decimal orderTotal, int customerId);

        /// <summary>Tính số tiền giảm của một voucher cho tổng đơn</summary>
        decimal CalculateDiscount(Voucher voucher, decimal orderTotal);

        /// <summary>
        /// Re-calculate cart: nếu newTotal < voucherMinOrderValue thì gỡ voucher.
        /// Trả về (discountAmount, voucherRemoved, message).
        /// </summary>
        Task<(decimal discount, bool voucherRemoved, string message)> RecalculateCartVoucherAsync(
            int voucherId, decimal newOrderTotal);

        /// <summary>Gán voucher auto cho user mới khi đăng ký (Welcome voucher)</summary>
        Task AssignWelcomeVoucherAsync(int customerId);

        /// <summary>Gán voucher thưởng từ sự kiện / minigame</summary>
        Task<bool> AssignRewardVoucherAsync(int customerId, int voucherId);

        // ---------- Checkout: commit voucher & points ----------
        /// <summary>
        /// Gọi trong ProcessOrder (trong transaction).
        /// Increment Voucher.UsedCount (row-level safe), đánh dấu UserVoucher.IsUsed.
        /// </summary>
        Task CommitVoucherUsageAsync(int voucherId, int customerId, int orderId);

        // ---------- Points ----------
        /// <summary>
        /// Cộng điểm khi đơn chuyển sang Delivered (StatusId=4).
        /// Guard: Order.PointsEarned == 0 (chưa từng cộng).
        /// Tự động upgrade tier nếu đủ điều kiện.
        /// Trả về số điểm được cộng.
        /// </summary>
        Task<int> AwardPointsAsync(int orderId);

        /// <summary>
        /// Trừ điểm khi đơn chuyển sang Cancelled (5) hoặc Returned (6).
        /// Guard: Order.PointsEarned > 0 (đã được cộng rồi mới trừ).
        /// </summary>
        Task RevokePointsAsync(int orderId);

        /// <summary>Dùng điểm tại checkout: trừ điểm khỏi customer, ghi lịch sử, trả về tiền giảm.</summary>
        Task<decimal> UsePointsAsync(int customerId, int pointsToUse);

        /// <summary>Cập nhật tier của customer dựa trên tổng điểm hiện tại.</summary>
        Task RefreshCustomerTierAsync(int customerId);

        /// <summary>Lấy thông tin tier hiện tại của customer</summary>
        Task<CustomerTier?> GetCustomerTierAsync(int customerId);

        /// <summary>
        /// Tặng voucher giảm 5% (tối đa 1.500.000đ, đơn tối thiểu 20 triệu, HSD 30 ngày)
        /// nếu đơn hàng vừa hoàn tất có tổng >= 20.000.000đ.
        /// Trả về true nếu voucher được tặng.
        /// </summary>
        Task<bool> AwardHighValueOrderVoucherAsync(int customerId, int orderId, decimal orderTotal);
    }
}
