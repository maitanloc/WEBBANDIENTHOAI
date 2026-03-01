// WEBBANDIENTHOAI/Services/PromotionService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly AppDbContext _ctx;

        // Tỷ lệ tích điểm: 10.000 VNĐ = 1 point
        private const decimal PointsPerVnd = 10_000m;
        // Quy đổi: 1 điểm = 1.000 VNĐ
        private const decimal VndPerPoint  = 1_000m;

        // Voucher ID mặc định cho welcome
        private const int WELCOME_VOUCHER_ID = 1;

        public PromotionService(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        // =================================================================
        //  VOUCHER VALIDATION
        // =================================================================

        public async Task<VoucherValidationResult> ValidateVoucherAsync(string code, decimal orderTotal, int customerId)
        {
            var result = new VoucherValidationResult();

            if (string.IsNullOrWhiteSpace(code))
            {
                result.Message = "Vui lòng nhập mã voucher.";
                return result;
            }

            var now = DateTime.UtcNow;

            var voucher = await _ctx.Vouchers
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Code == code.Trim().ToUpper() && v.IsActive);

            if (voucher == null)
            {
                result.Message = "Mã voucher không tồn tại hoặc đã bị vô hiệu hóa.";
                return result;
            }

            if (now < voucher.StartDate || now > voucher.EndDate)
            {
                result.Message = "Mã voucher đã hết hạn hoặc chưa đến ngày áp dụng.";
                return result;
            }

            // Kiểm tra còn lượt (Quantity = 0 nghĩa là không giới hạn)
            if (voucher.Quantity > 0 && voucher.UsedCount >= voucher.Quantity)
            {
                result.Message = "Mã voucher đã hết lượt sử dụng.";
                return result;
            }

            if (orderTotal < voucher.MinOrderValue)
            {
                result.Message = $"Đơn hàng phải đạt tối thiểu {voucher.MinOrderValue:N0}đ để áp dụng mã này.";
                return result;
            }

            // Kiểm tra UserVoucher: nếu voucher thuộc kho cá nhân thì chỉ chủ sở hữu mới dùng được
            var isPersonal = await _ctx.UserVouchers
                .AnyAsync(uv => uv.VoucherId == voucher.VoucherId && !uv.IsUsed);

            if (isPersonal)
            {
                var ownerExists = await _ctx.UserVouchers
                    .AnyAsync(uv => uv.VoucherId == voucher.VoucherId
                                 && uv.CustomerId == customerId
                                 && !uv.IsUsed);
                if (!ownerExists)
                {
                    result.Message = "Mã voucher này không thuộc về tài khoản của bạn.";
                    return result;
                }
            }

            result.IsValid       = true;
            result.Voucher       = voucher;
            result.DiscountAmount = CalculateDiscount(voucher, orderTotal);
            result.Message       = $"Áp dụng thành công! Bạn được giảm {result.DiscountAmount:N0}đ.";
            return result;
        }

        public decimal CalculateDiscount(Voucher voucher, decimal orderTotal)
        {
            if (voucher.DiscountType == DiscountType.Fixed)
            {
                return Math.Min(voucher.Value, orderTotal);
            }
            else // Percent
            {
                decimal discount = orderTotal * voucher.Value / 100m;
                if (voucher.MaxDiscountAmount.HasValue)
                    discount = Math.Min(discount, voucher.MaxDiscountAmount.Value);
                return Math.Round(discount, 0);
            }
        }

        // =================================================================
        //  RE-CALCULATE CART VOUCHER
        // =================================================================

        public async Task<(decimal discount, bool voucherRemoved, string message)> RecalculateCartVoucherAsync(
            int voucherId, decimal newOrderTotal)
        {
            var voucher = await _ctx.Vouchers.AsNoTracking()
                .FirstOrDefaultAsync(v => v.VoucherId == voucherId && v.IsActive);

            if (voucher == null)
                return (0m, true, "Voucher không còn hợp lệ.");

            if (newOrderTotal < voucher.MinOrderValue)
                return (0m, true,
                    $"Đơn hàng không còn đủ điều kiện áp dụng mã [{voucher.Code}]. " +
                    $"Cần tối thiểu {voucher.MinOrderValue:N0}đ.");

            return (CalculateDiscount(voucher, newOrderTotal), false, string.Empty);
        }

        // =================================================================
        //  VOUCHER ASSIGNMENT
        // =================================================================

        public async Task AssignWelcomeVoucherAsync(int customerId)
        {
            bool alreadyHas = await _ctx.UserVouchers
                .AnyAsync(uv => uv.CustomerId == customerId && uv.VoucherId == WELCOME_VOUCHER_ID);

            if (!alreadyHas)
            {
                _ctx.UserVouchers.Add(new UserVoucher
                {
                    CustomerId  = customerId,
                    VoucherId   = WELCOME_VOUCHER_ID,
                    AssignedAt  = DateTime.UtcNow,
                    IsUsed      = false
                });
                await _ctx.SaveChangesAsync();
            }
        }

        public async Task<bool> AssignRewardVoucherAsync(int customerId, int voucherId)
        {
            var voucher = await _ctx.Vouchers.FirstOrDefaultAsync(v => v.VoucherId == voucherId && v.IsActive);
            if (voucher == null) return false;

            _ctx.UserVouchers.Add(new UserVoucher
            {
                CustomerId = customerId,
                VoucherId  = voucherId,
                AssignedAt = DateTime.UtcNow,
                IsUsed     = false
            });
            await _ctx.SaveChangesAsync();
            return true;
        }

        // =================================================================
        //  COMMIT VOUCHER USAGE
        // =================================================================

        public async Task CommitVoucherUsageAsync(int voucherId, int customerId, int orderId)
        {
            await _ctx.Vouchers
                .Where(v => v.VoucherId == voucherId
                         && (v.Quantity == 0 || v.UsedCount < v.Quantity))
                .ExecuteUpdateAsync(s => s.SetProperty(v => v.UsedCount, v => v.UsedCount + 1));

            var uv = await _ctx.UserVouchers
                .FirstOrDefaultAsync(uv => uv.CustomerId == customerId
                                        && uv.VoucherId  == voucherId
                                        && !uv.IsUsed);
            if (uv != null)
            {
                uv.IsUsed  = true;
                uv.UsedAt  = DateTime.UtcNow;
                uv.OrderId = orderId;
            }

            await _ctx.SaveChangesAsync();
        }

        // =================================================================
        //  POINTS – AWARD (khi Delivered)
        //  - Mỗi đơn hàng được giao thành công đều cộng điểm
        //  - Guard PointsEarned > 0 đảm bảo không cộng 2 lần cùng đơn
        // =================================================================

        public async Task<int> AwardPointsAsync(int orderId)
        {
            try
            {
                // Clear EF identity cache để tránh stale entity sau UpdateStatusAsync
                _ctx.ChangeTracker.Clear();

                var order = await _ctx.Orders
                    .Include(o => o.Customer)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null)
                {
                    Console.WriteLine($"[Loyalty] Không tìm thấy Order #{orderId}");
                    return 0;
                }

                // Guard: chỉ cộng 1 lần / 1 đơn hàng
                if (order.PointsEarned > 0)
                {
                    Console.WriteLine($"[Loyalty] Order #{orderId} đã cộng {order.PointsEarned} điểm rồi, bỏ qua.");
                    return 0;
                }

                var customer = order.Customer
                    ?? await _ctx.Customers.FindAsync(order.CustomerId);

                if (customer == null)
                {
                    Console.WriteLine($"[Loyalty] Không tìm thấy Customer #{order.CustomerId}");
                    return 0;
                }

                // Tăng tổng chi tiêu tích lũy (TotalSpent)
                customer.TotalSpent += order.Total;

                // Tổng thực tế sau giảm giá + điểm đã dùng (để tính điểm thưởng)
                decimal payableAmount = order.Total - order.DiscountAmount
                                        - (order.PointsUsed * VndPerPoint);
                if (payableAmount < 0) payableAmount = 0;

                // Nhân hệ số theo tier hiện tại của customer
                int tierId = customer.TierId;
                var tier = await _ctx.CustomerTiers
                    .FirstOrDefaultAsync(t => t.TierId == tierId);
                decimal multiplier = tier?.BonusMultiplier ?? 1.0m;

                int pointsToEarn = (int)Math.Floor(payableAmount / PointsPerVnd * multiplier);
                
                // Cập nhật điểm và lưu vào Order (pointsToEarn có thể là 0 nhưng vẫn phải lưu TotalSpent)
                customer.LoyaltyPoints += pointsToEarn;
                order.PointsEarned = pointsToEarn;

                if (pointsToEarn > 0)
                {
                    // Ghi lịch sử giao dịch điểm
                    _ctx.UserPointHistories.Add(new UserPointHistory
                    {
                        CustomerId = customer.CustomerId,
                        OrderId    = orderId,
                        Points     = pointsToEarn,
                        Reason     = $"Tích điểm đơn hàng #{orderId} - Giao thành công",
                        CreatedAt  = DateTime.UtcNow
                    });
                }

                await _ctx.SaveChangesAsync();

                Console.WriteLine($"[Loyalty] ✅ Order #{orderId}: +{pointsToEarn} điểm, +{order.Total:N0}đ Chi tiêu cho Customer #{customer.CustomerId}");
                Console.WriteLine($"[Loyalty]    -> Tổng chi tiêu: {customer.TotalSpent:N0}đ, Tổng điểm: {customer.LoyaltyPoints}");

                // Sau khi lưu, refresh tier (tự động thăng hạng theo TotalSpent)
                await RefreshCustomerTierAsync(customer.CustomerId);

                return pointsToEarn;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Loyalty] ❌ Lỗi AwardPointsAsync Order #{orderId}: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[Loyalty]    Inner: {ex.InnerException.Message}");
                return 0;
            }
        }

        // =================================================================
        //  POINTS – REVOKE (khi Cancelled / Returned)
        // =================================================================

        public async Task RevokePointsAsync(int orderId)
        {
            try
            {
                _ctx.ChangeTracker.Clear();

                var order = await _ctx.Orders
                    .Include(o => o.Customer)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (order == null) return;

                var customer = order.Customer
                    ?? await _ctx.Customers.FindAsync(order.CustomerId);
                if (customer == null) return;

                // Hoàn tác tổng chi tiêu
                customer.TotalSpent = Math.Max(0, customer.TotalSpent - order.Total);

                int pointsToRevoke = order.PointsEarned;
                if (pointsToRevoke > 0)
                {
                    customer.LoyaltyPoints = Math.Max(0, customer.LoyaltyPoints - pointsToRevoke);
                    order.PointsEarned = 0;

                    _ctx.UserPointHistories.Add(new UserPointHistory
                    {
                        CustomerId = customer.CustomerId,
                        OrderId    = orderId,
                        Points     = -pointsToRevoke,
                        Reason     = $"Thu hồi điểm đơn hàng #{orderId} - Hủy/Trả hàng",
                        CreatedAt  = DateTime.UtcNow
                    });
                }

                await _ctx.SaveChangesAsync();
                Console.WriteLine($"[Loyalty] Thu hồi {pointsToRevoke} điểm và {order.Total:N0}đ chi tiêu của Customer #{customer.CustomerId}, Đơn #{orderId}");

                await RefreshCustomerTierAsync(customer.CustomerId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Loyalty] ❌ Lỗi RevokePointsAsync Order #{orderId}: {ex.Message}");
            }
        }

        // =================================================================
        //  POINTS – USE AT CHECKOUT
        // =================================================================

        public async Task<decimal> UsePointsAsync(int customerId, int pointsToUse)
        {
            if (pointsToUse <= 0) return 0m;

            var customer = await _ctx.Customers.FindAsync(customerId);
            if (customer == null) return 0m;

            if (pointsToUse > customer.LoyaltyPoints)
                pointsToUse = customer.LoyaltyPoints;

            decimal moneyValue = pointsToUse * VndPerPoint;
            customer.LoyaltyPoints -= pointsToUse;

            _ctx.UserPointHistories.Add(new UserPointHistory
            {
                CustomerId = customerId,
                Points     = -pointsToUse,
                Reason     = "Dùng điểm thanh toán tại checkout",
                CreatedAt  = DateTime.UtcNow
            });

            await _ctx.SaveChangesAsync();
            return moneyValue;
        }

        // =================================================================
        //  TIER REFRESH – tự động thăng/hạ dựa trên TỔNG CHI TIÊU (TotalSpent)
        // =================================================================

        public async Task RefreshCustomerTierAsync(int customerId)
        {
            try
            {
                // Clear cache để đọc fresh data sau SaveChanges bên AwardPoints
                _ctx.ChangeTracker.Clear();

                var customer = await _ctx.Customers
                    .Include(c => c.Tier)
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId);
                if (customer == null) return;

                // Lấy tất cả tiers, sắp xếp từ chi tiêu cao xuống thấp
                var tiers = await _ctx.CustomerTiers
                    .OrderByDescending(t => t.MinSpending)
                    .ToListAsync();

                // Tìm tier phù hợp cao nhất dựa trên TotalSpent
                var newTier = tiers.FirstOrDefault(t => customer.TotalSpent >= t.MinSpending);
                if (newTier == null) return;

                bool tierChanged = customer.TierId != newTier.TierId;
                if (tierChanged)
                {
                    customer.TierId = newTier.TierId;
                    await _ctx.SaveChangesAsync();
                    Console.WriteLine($"[Loyalty] 🏆 Customer #{customerId} thăng hạng → {newTier.TierName} (Tổng chi tiêu: {customer.TotalSpent:N0}đ)");

                    // Tặng 20 voucher thưởng khi thăng hạng
                    await AssignTierUpgradeVouchersAsync(customerId, newTier);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Loyalty] ❌ Lỗi RefreshCustomerTierAsync Customer #{customerId}: {ex.Message}");
            }
        }

        private async Task AssignTierUpgradeVouchersAsync(int customerId, CustomerTier newTier)
        {
            try
            {
                string tierName = newTier.TierName.ToUpper().Replace(" ", "_");
                // Thêm CustomerId vào prefix để mã voucher là duy nhất cho mỗi khách hàng
                string codePrefix = $"UPG_{tierName}_C{customerId}_{DateTime.UtcNow:yyMM}";

                // Base values theo hạng
                decimal baseFixed = tierName.Contains("DIAMOND") || tierName.Contains("KIM") ? 200000m
                                  : tierName.Contains("GOLD")    || tierName.Contains("VÀNG")  ? 100000m
                                  : tierName.Contains("SILVER")  || tierName.Contains("BẠC")   ? 50000m
                                  : 20000m;

                decimal basePercent = tierName.Contains("DIAMOND") || tierName.Contains("KIM") ? 20m
                                    : tierName.Contains("GOLD")    || tierName.Contains("VÀNG")  ? 15m
                                    : tierName.Contains("SILVER")  || tierName.Contains("BẠC")   ? 10m
                                    : 5m;

                var vouchersToCreate = new List<Voucher>();

                for (int i = 1; i <= 20; i++)
                {
                    string vCode = $"{codePrefix}_V{i:D2}";

                    bool exists = await _ctx.Vouchers.AnyAsync(x => x.Code == vCode);
                    if (exists) continue;

                    bool isPercent = (i % 2 == 0);
                    Voucher v;

                    if (isPercent)
                    {
                        decimal pct = basePercent + ((i / 2 - 1) % 4) * 5m;
                        pct = Math.Min(pct, 50m);
                        decimal maxDisc = baseFixed * (1 + (i % 3));
                        decimal minOrd  = baseFixed * 3;

                        v = new Voucher
                        {
                            Code             = vCode,
                            Description      = $"Giảm {pct}% (tối đa {maxDisc:N0}đ, đơn ≥{minOrd:N0}đ) - Hạng {newTier.TierName}",
                            DiscountType     = DiscountType.Percent,
                            Value            = pct,
                            MaxDiscountAmount = maxDisc,
                            MinOrderValue    = minOrd,
                            Quantity         = 1,
                            UsedCount        = 0,
                            IsActive         = true,
                            CreatedAt        = DateTime.UtcNow,
                            StartDate        = DateTime.UtcNow,
                            EndDate          = DateTime.UtcNow.AddMonths(3)
                        };
                    }
                    else
                    {
                        decimal fixedVal = baseFixed + ((i - 1) / 2) * (baseFixed * 0.5m);
                        decimal minOrd   = fixedVal * 2;

                        v = new Voucher
                        {
                            Code             = vCode,
                            Description      = $"Giảm {fixedVal:N0}đ (đơn ≥{minOrd:N0}đ) - Hạng {newTier.TierName}",
                            DiscountType     = DiscountType.Fixed,
                            Value            = fixedVal,
                            MaxDiscountAmount = null,
                            MinOrderValue    = minOrd,
                            Quantity         = 1,
                            UsedCount        = 0,
                            IsActive         = true,
                            CreatedAt        = DateTime.UtcNow,
                            StartDate        = DateTime.UtcNow,
                            EndDate          = DateTime.UtcNow.AddMonths(3)
                        };
                    }

                    vouchersToCreate.Add(v);
                    _ctx.Vouchers.Add(v);
                }

                // Lưu tất cả voucher mới để EF cấp VoucherId
                if (vouchersToCreate.Any())
                    await _ctx.SaveChangesAsync();

                // Gán vào kho voucher của user
                int assigned = 0;
                foreach (var v in vouchersToCreate)
                {
                    if (v.VoucherId <= 0) continue;

                    bool alreadyHas = await _ctx.UserVouchers
                        .AnyAsync(uv => uv.CustomerId == customerId && uv.VoucherId == v.VoucherId);

                    if (!alreadyHas)
                    {
                        _ctx.UserVouchers.Add(new UserVoucher
                        {
                            CustomerId = customerId,
                            VoucherId  = v.VoucherId,
                            AssignedAt = DateTime.UtcNow,
                            IsUsed     = false
                        });
                        assigned++;
                    }
                }

                if (assigned > 0)
                {
                    await _ctx.SaveChangesAsync();
                    Console.WriteLine($"[Loyalty] 🎁 Tặng {assigned} voucher thăng hạng cho Customer #{customerId} - Hạng {newTier.TierName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Loyalty] ❌ Lỗi AssignTierUpgradeVouchers: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"[Loyalty]    Inner: {ex.InnerException.Message}");
            }
        }

        public async Task<CustomerTier?> GetCustomerTierAsync(int customerId)
        {
            var customer = await _ctx.Customers
                .AsNoTracking()
                .Include(c => c.Tier)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
            return customer?.Tier;
        }

        // =================================================================
        //  HIGH-VALUE ORDER VOUCHER – Tặng voucher 5% khi đơn >= 20 triệu
        // =================================================================

        private const decimal HighValueOrderThreshold = 20_000_000m;
        private const decimal HighValueVoucherPercent  = 5m;
        private const decimal HighValueMaxDiscount     = 1_500_000m;
        private const int     HighValueVoucherDays     = 30;

        public async Task<bool> AwardHighValueOrderVoucherAsync(int customerId, int orderId, decimal orderTotal)
        {
            try
            {
                if (orderTotal < HighValueOrderThreshold)
                    return false;

                string voucherCode = $"HV5_{customerId}_{orderId}";

                // Tránh tặng trùng (trường hợp callback gọi nhiều lần)
                bool alreadyExists = await _ctx.Vouchers.AnyAsync(v => v.Code == voucherCode);
                if (alreadyExists)
                {
                    Console.WriteLine($"[HV Voucher] Voucher {voucherCode} đã tồn tại, bỏ qua.");
                    return false;
                }

                var now = DateTime.UtcNow;

                var voucher = new Voucher
                {
                    Code              = voucherCode,
                    Description       = $"Thưởng đơn hàng #{orderId} – Giảm 5% (tối đa 1.500.000đ, đơn ≥20.000.000đ)",
                    DiscountType      = DiscountType.Percent,
                    Value             = HighValueVoucherPercent,
                    MaxDiscountAmount = HighValueMaxDiscount,
                    MinOrderValue     = HighValueOrderThreshold,
                    Quantity          = 1,
                    UsedCount         = 0,
                    IsActive          = true,
                    CreatedAt         = now,
                    StartDate         = now,
                    EndDate           = now.AddDays(HighValueVoucherDays)
                };

                _ctx.Vouchers.Add(voucher);
                await _ctx.SaveChangesAsync(); // Lấy VoucherId

                _ctx.UserVouchers.Add(new UserVoucher
                {
                    CustomerId = customerId,
                    VoucherId  = voucher.VoucherId,
                    AssignedAt = now,
                    IsUsed     = false
                });
                await _ctx.SaveChangesAsync();

                Console.WriteLine($"[HV Voucher] 🎁 Tặng voucher {voucherCode} (giảm 5%, tối đa 1.5tr) cho Customer #{customerId}, Đơn #{orderId} – {orderTotal:N0}đ");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HV Voucher] ❌ Lỗi AwardHighValueOrderVoucherAsync: {ex.Message}");
                return false;
            }
        }
    }
}
