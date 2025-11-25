using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using Microsoft.EntityFrameworkCore;

namespace WEBBANDIENTHOAI.Services
{
    public class OTPService : IOTPService
    {
        private readonly IMemoryCache _cache;
        private readonly AppDbContext _context;
        private readonly Random _random;

        // Constructor chỉ nhận 2 parameters
        public OTPService(IMemoryCache cache, AppDbContext context)
        {
            _cache = cache;
            _context = context;
            _random = new Random();
        }

        // Các method khác giữ nguyên...
        public string GenerateOTP()
        {
            return _random.Next(100000, 999999).ToString();
        }

        public async Task StoreOTPAsync(string email, string otp)
        {
            // Lưu vào cache (cho performance)
            var cacheKey = $"OTP_{email}";
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(10));

            _cache.Set(cacheKey, otp, cacheEntryOptions);

            // Lưu vào database (cho persistence)
            var otpCode = new OTPCode
            {
                Code = otp,
                Email = email,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };

            _context.OTPCodes.Add(otpCode);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ValidateOTPAsync(string email, string otp)
        {
            // Kiểm tra trong cache trước (nhanh)
            var cacheKey = $"OTP_{email}";
            if (_cache.TryGetValue(cacheKey, out string storedOTP))
            {
                if (storedOTP == otp)
                {
                    // Xóa OTP khỏi cache sau khi xác thực thành công
                    _cache.Remove(cacheKey);

                    // Đánh dấu đã sử dụng trong database
                    await MarkOTPAsUsed(email, otp);
                    return true;
                }
            }

            // Nếu không có trong cache, kiểm tra database
            var otpRecord = await _context.OTPCodes
                .FirstOrDefaultAsync(o => o.Email == email && o.Code == otp && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow);

            if (otpRecord != null)
            {
                // Đánh dấu đã sử dụng
                otpRecord.IsUsed = true;
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        private async Task MarkOTPAsUsed(string email, string otp)
        {
            var otpRecord = await _context.OTPCodes
                .FirstOrDefaultAsync(o => o.Email == email && o.Code == otp && !o.IsUsed);

            if (otpRecord != null)
            {
                otpRecord.IsUsed = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}