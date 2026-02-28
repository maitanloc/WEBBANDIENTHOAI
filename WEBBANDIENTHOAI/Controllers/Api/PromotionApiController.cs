// WEBBANDIENTHOAI/Controllers/Api/PromotionApiController.cs
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WEBBANDIENTHOAI.Services;

namespace WEBBANDIENTHOAI.Controllers.Api
{
    /// <summary>
    /// Stateless API endpoint – dành cho minigame frontend gọi vào để cấp voucher.
    /// Bảo mật bằng API Key trong appsettings.json["PromotionApiKey"].
    /// POST /api/promotion/reward
    /// Body: { "customerId": 1, "voucherId": 2, "apiKey": "SECRET" }
    /// </summary>
    [ApiController]
    [Route("api/promotion")]
    public class PromotionApiController : ControllerBase
    {
        private readonly IPromotionService _svc;
        private readonly string _apiKey;

        public PromotionApiController(IPromotionService svc, IConfiguration cfg)
        {
            _svc   = svc;
            _apiKey = cfg["PromotionApiKey"] ?? "CHANGE_ME_IN_APPSETTINGS";
        }

        // POST /api/promotion/reward
        [HttpPost("reward")]
        public async Task<IActionResult> Reward([FromBody] RewardRequest req)
        {
            // 1. Validate API Key
            if (string.IsNullOrWhiteSpace(req?.ApiKey) || req.ApiKey != _apiKey)
                return Unauthorized(new { success = false, message = "API key không hợp lệ." });

            // 2. Validate params
            if (req.CustomerId <= 0 || req.VoucherId <= 0)
                return BadRequest(new { success = false, message = "CustomerId và VoucherId phải > 0." });

            // 3. Assign voucher
            bool ok = await _svc.AssignRewardVoucherAsync(req.CustomerId, req.VoucherId);
            if (!ok)
                return BadRequest(new { success = false, message = "Voucher không tồn tại hoặc đã bị vô hiệu hóa." });

            return Ok(new { success = true, message = $"Đã cấp voucher #{req.VoucherId} cho khách hàng #{req.CustomerId}." });
        }

        // POST /api/promotion/award-points (dùng nội bộ nếu cần test)
        [HttpPost("award-points")]
        public async Task<IActionResult> AwardPoints([FromBody] AwardPointsRequest req)
        {
            if (string.IsNullOrWhiteSpace(req?.ApiKey) || req.ApiKey != _apiKey)
                return Unauthorized(new { success = false, message = "API key không hợp lệ." });

            await _svc.AwardPointsAsync(req.OrderId);
            return Ok(new { success = true, message = $"Đã xử lý điểm cho đơn #{req.OrderId}." });
        }
    }

    public class RewardRequest
    {
        public int    CustomerId { get; set; }
        public int    VoucherId  { get; set; }
        public string ApiKey     { get; set; } = string.Empty;
    }

    public class AwardPointsRequest
    {
        public int    OrderId { get; set; }
        public string ApiKey  { get; set; } = string.Empty;
    }
}
