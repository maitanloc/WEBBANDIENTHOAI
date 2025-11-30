using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using WEBBANDIENTHOAI.Models;
using WEBBANDIENTHOAI.Data;

namespace WEBBANDIENTHOAI.Controllers.NguoiDung
{
    public class ChatController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly string _apiKey;
        private const string ApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

        // ĐỊNH NGHĨA NHÂN CÁCH FOX AI
        private const string SystemInstruction = @"
            Bạn tên là Fox AI, chuyên gia tư vấn bán hàng điện thoại và laptop của FOXMOBILE.
            
            NGUYÊN TẮC TƯ VẤN:
            1. Trả lời thân thiện, nhiệt tình, ngắn gọn (2-4 câu). Xưng 'Fox AI' hoặc 'mình', gọi khách là 'bạn'.
            2. Phân tích nhu cầu: giá, cấu hình (CPU, RAM, bộ nhớ), mục đích sử dụng (gaming, văn phòng, đồ họa).
            3. Ưu tiên sản phẩm CÒN HÀNG (CurrentQuantity > 0), sau đó mới đề xuất sản phẩm hết hàng.
            4. So sánh 2-3 sản phẩm phù hợp nhất dựa trên: giá, cấu hình, tồn kho.
            5. Luôn đề cập tình trạng kho: 'Hiện còn X cái' hoặc 'Tạm hết hàng'.
            6. Kết thúc bằng câu hỏi mở hoặc lời mời đặt hàng.
            7. TUYỆT ĐỐI KHÔNG bịa đặt thông tin không có trong dữ liệu.
            
            CÁCH TRẢ LỜI MẪU:
            - Nếu khách hỏi 'laptop gaming 30 triệu': 
              'Mình gợi ý 2 máy gaming trong tầm 30tr bạn nhé: [Tên SP1] (giá, CPU, RAM, tồn kho) và [Tên SP2]. Bạn chơi game nào chủ yếu để mình tư vấn kỹ hơn?'
            - Nếu hỏi 'iPhone còn không?':
              'iPhone 15 Pro Max còn X cái bạn nhé, giá [giá]. Bạn quan tâm bộ nhớ nào ạ?'
        ";

        public ChatController(HttpClient httpClient, IConfiguration configuration, AppDbContext context)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _context = context;
            _apiKey = _configuration.GetValue<string>("AppSettings:Gemini:ApiKey") ?? string.Empty;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] AskRequest request)
        {
            if (string.IsNullOrEmpty(request.UserPrompt) || string.IsNullOrEmpty(_apiKey))
            {
                return Json(new { success = false, message = "Vui lòng nhập câu hỏi." });
            }

            try
            {
                // BƯỚC 1: LẤY LỊCH SỬ CHAT
                var historyJson = HttpContext.Session.GetString("ChatHistory") ?? JsonSerializer.Serialize(new List<ChatMessage>());
                var history = JsonSerializer.Deserialize<List<ChatMessage>>(historyJson)!;

                var contents = new List<object>();
                foreach (var message in history)
                {
                    contents.Add(new { role = "user", parts = new[] { new { text = message.UserPrompt } } });
                    contents.Add(new { role = "model", parts = new[] { new { text = message.AiResponse } } });
                }

                // BƯỚC 2: LẤY DỮ LIỆU TƯ VẤN TỪ DATABASE (RAG)
                string contextData = await GetConsultingContextFromDB(request.UserPrompt!);

                var finalPrompt = string.IsNullOrWhiteSpace(contextData)
                    ? $"Câu hỏi: {request.UserPrompt}\n(Không tìm thấy sản phẩm phù hợp, hãy hỏi thêm để tư vấn chính xác)"
                    : $"DỮ LIỆU SẢN PHẨM:\n{contextData}\n\nCÂU HỎI KHÁCH HÀNG: {request.UserPrompt}\n\nHãy tư vấn dựa trên dữ liệu trên.";

                contents.Add(new { role = "user", parts = new[] { new { text = finalPrompt } } });

                // BƯỚC 3: GỌI GEMINI API
                var requestBody = new
                {
                    system_instruction = new { parts = new[] { new { text = SystemInstruction } } },
                    contents = contents.ToArray()
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(ApiUrl + "?key=" + _apiKey, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    using (JsonDocument doc = JsonDocument.Parse(responseString))
                    {
                        var text = doc.RootElement
                            .GetProperty("candidates")[0]
                            .GetProperty("content")
                            .GetProperty("parts")[0]
                            .GetProperty("text")
                            .GetString();

                        // BƯỚC 4: LƯU LỊCH SỬ (GIỮ 20 TIN NHẮN GẦN NHẤT)
                        history.Add(new ChatMessage
                        {
                            UserPrompt = request.UserPrompt!,
                            AiResponse = text!,
                            Timestamp = DateTime.Now
                        });
                        HttpContext.Session.SetString("ChatHistory", JsonSerializer.Serialize(history.TakeLast(20).ToList()));

                        return Json(new { success = true, aiResponse = text });
                    }
                }

                return Json(new { success = false, message = $"Lỗi API: {response.StatusCode}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        public IActionResult GetChatHistory()
        {
            var historyJson = HttpContext.Session.GetString("ChatHistory");
            var history = string.IsNullOrEmpty(historyJson)
                ? new List<ChatMessage>()
                : JsonSerializer.Deserialize<List<ChatMessage>>(historyJson);

            return Json(history);
        }

        [HttpPost]
        public IActionResult ClearHistory()
        {
            HttpContext.Session.Remove("ChatHistory");
            return Json(new { success = true });
        }

        // ===================== HÀM TƯ VẤN THỰC TẾ TỪ DATABASE =====================
        private async Task<string> GetConsultingContextFromDB(string userMessage)
        {
            var result = new StringBuilder();
            var lowerMsg = userMessage.ToLower();

            try
            {
                // TEST: Thử query đơn giản nhất trước
                var totalProducts = await _context.Products.CountAsync();
                if (totalProducts == 0)
                {
                    return "❌ Database không có sản phẩm nào. Vui lòng thêm sản phẩm trước.";
                }

                Console.WriteLine($"[ChatController] Tổng số sản phẩm: {totalProducts}");

                // XÁC ĐỊNH CATEGORY: Laptop hay Điện thoại
                var isLaptop = lowerMsg.Contains("laptop") || lowerMsg.Contains("máy tính");
                var isPhone = lowerMsg.Contains("điện thoại") || lowerMsg.Contains("iphone") ||
                              lowerMsg.Contains("samsung") || lowerMsg.Contains("oppo") ||
                              lowerMsg.Contains("xiaomi") || lowerMsg.Contains("phone");

                // TÌM KIẾM SẢN PHẨM - SỬA: Không dùng Include để tránh lỗi navigation
                var query = _context.Products
                    .Where(p => p.StatusId == 1) // Chỉ lấy sản phẩm đang kinh doanh
                    .AsNoTracking() // Tăng performance
                    .AsQueryable();

                // LỌC THEO CATEGORY - SỬA: Bỏ qua nếu không xác định được
                int? targetCategoryId = null;
                if (isLaptop && !isPhone)
                {
                    targetCategoryId = 2; // Kiểm tra CategoryId trong DB
                }
                else if (isPhone && !isLaptop)
                {
                    targetCategoryId = 1;
                }

                if (targetCategoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == targetCategoryId.Value);
                }

                // LỌC THEO TÊN/BRAND
                var keywords = ExtractKeywords(lowerMsg);
                if (keywords.Any())
                {
                    query = query.Where(p =>
                        keywords.Any(k => p.Name.ToLower().Contains(k) ||
                                         p.Brand != null && p.Brand.ToLower().Contains(k)));
                }

                // LỌC THEO GIÁ
                var (minPrice, maxPrice) = ExtractPriceRange(lowerMsg);
                if (minPrice.HasValue)
                    query = query.Where(p => p.Price >= minPrice.Value);
                if (maxPrice.HasValue)
                    query = query.Where(p => p.Price <= maxPrice.Value);

                // ƯU TIÊN SẢN PHẨM CÒN HÀNG, GIỚI HẠN 5 SẢN PHẨM
                var products = await query
                    .OrderBy(p => p.Price)
                    .Take(10)
                    .ToListAsync();

                Console.WriteLine($"[ChatController] Tìm thấy {products.Count} sản phẩm");

                if (!products.Any())
                {
                    return "Không tìm thấy sản phẩm phù hợp. Bạn có thể mô tả rõ hơn nhu cầu không?";
                }

                // SỬA: Lấy thông tin bổ sung - XỬ LÝ AN TOÀN
                var productIds = products.Select(p => p.ProductId).ToList();

                // Lấy Inventory
                Dictionary<int, int> inventories;
                try
                {
                    inventories = await _context.Inventory
                        .Where(i => i.ProductId != null && productIds.Contains(i.ProductId.Value))
                        .GroupBy(i => i.ProductId)
                        .Select(g => new { ProductId = g.Key!.Value, TotalStock = g.Sum(i => i.CurrentQuantity) })
                        .ToDictionaryAsync(x => x.ProductId, x => x.TotalStock);
                }
                catch
                {
                    inventories = new Dictionary<int, int>();
                }

                // Lấy LaptopConfiguration
                Dictionary<int, LaptopConfiguration> laptopConfigs;
                try
                {
                    laptopConfigs = await _context.LaptopConfigurations
                        .Where(lc => productIds.Contains(lc.ProductId))
                        .ToDictionaryAsync(lc => lc.ProductId);
                }
                catch
                {
                    laptopConfigs = new Dictionary<int, LaptopConfiguration>();
                }

                // Lấy PhoneConfiguration
                Dictionary<int, PhoneConfiguration> phoneConfigs;
                try
                {
                    phoneConfigs = await _context.PhoneConfigurations
                        .Where(pc => productIds.Contains(pc.ProductId))
                        .ToDictionaryAsync(pc => pc.ProductId);
                }
                catch
                {
                    phoneConfigs = new Dictionary<int, PhoneConfiguration>();
                }

                // Sắp xếp ưu tiên sản phẩm còn hàng
                products = products
                    .OrderByDescending(p => inventories.GetValueOrDefault(p.ProductId, 0))
                    .ThenBy(p => p.Price)
                    .Take(5)
                    .ToList();

                // TẠO DỮ LIỆU CONTEXT
                if (!products.Any())
                {
                    return "Không tìm thấy sản phẩm phù hợp trong kho.";
                }

                result.AppendLine("=== SẢN PHẨM PHÙ HỢP ===");
                foreach (var product in products)
                {
                    // Lấy số lượng tồn kho
                    var stockQuantity = inventories.GetValueOrDefault(product.ProductId, 0);
                    var stockStatus = stockQuantity > 0 ? $"Còn {stockQuantity} cái" : "Tạm hết hàng";

                    result.AppendLine($"\n📱 {product.Name}");
                    result.AppendLine($"   - Giá: {product.Price:N0} VND" + (product.OldPrice.HasValue ? $" (Giá cũ: {product.OldPrice:N0} VND)" : ""));
                    result.AppendLine($"   - Tình trạng: {stockStatus}");
                    result.AppendLine($"   - Mã SP: {product.SKU}");

                    // CẤU HÌNH LAPTOP
                    if (laptopConfigs.TryGetValue(product.ProductId, out var laptop))
                    {
                        result.AppendLine($"   - CPU: {laptop.CPU ?? "N/A"}");
                        result.AppendLine($"   - RAM: {laptop.RAM ?? "N/A"}");
                        result.AppendLine($"   - Ổ cứng: {laptop.Storage ?? "N/A"}");
                        result.AppendLine($"   - Card đồ họa: {laptop.GraphicsCard ?? "N/A"}");
                        result.AppendLine($"   - Màn hình: {laptop.ScreenSize ?? "N/A"} - {laptop.Resolution ?? "N/A"}");
                    }

                    // CẤU HÌNH ĐIỆN THOẠI
                    if (phoneConfigs.TryGetValue(product.ProductId, out var phone))
                    {
                        result.AppendLine($"   - CPU: {phone.CPU ?? "N/A"}");
                        result.AppendLine($"   - RAM: {phone.RAM ?? "N/A"}");
                        result.AppendLine($"   - Bộ nhớ: {phone.InternalStorage ?? "N/A"}");
                        result.AppendLine($"   - Pin: {phone.Battery ?? "N/A"}");
                        result.AppendLine($"   - Màn hình: {phone.Screen ?? "N/A"} - {phone.Resolution ?? "N/A"}");
                        result.AppendLine($"   - Camera: {phone.Camera ?? "N/A"}");
                    }

                    if (!string.IsNullOrWhiteSpace(product.ShortDescription))
                    {
                        result.AppendLine($"   - Mô tả: {product.ShortDescription}");
                    }
                }

                // THÊM CHÍNH SÁCH
                result.AppendLine("\n=== CHÍNH SÁCH FOXMOBILE ===");
                result.AppendLine("✅ Bảo hành 12 tháng chính hãng");
                result.AppendLine("✅ Miễn phí giao hàng toàn quốc (COD 30k)");
                result.AppendLine("✅ Đổi trả trong 7 ngày nếu có lỗi");
                result.AppendLine("✅ Giảm 5% cho đơn hàng trên 50 triệu");

                return result.ToString();
            }
            catch (Exception ex)
            {
                // LOG CHI TIẾT ĐỂ DEBUG
                Console.WriteLine($"[ChatController Error] {ex.Message}");
                Console.WriteLine($"[ChatController StackTrace] {ex.StackTrace}");

                // FALLBACK: Dùng dữ liệu tĩnh tạm thời
                return GetStaticProductData(lowerMsg);
            }
        }

        // PHƯƠNG ÁN DỰ PHÒNG: Dữ liệu tĩnh để test
        private string GetStaticProductData(string query)
        {
            var result = new StringBuilder();
            result.AppendLine("=== SẢN PHẨM GỢI Ý (DỮ LIỆU MẪU) ===\n");

            if (query.Contains("iphone"))
            {
                result.AppendLine("📱 iPhone 15 Pro Max");
                result.AppendLine("   - Giá: 28.000.000 VND");
                result.AppendLine("   - Tình trạng: Còn 5 cái");
                result.AppendLine("   - CPU: Apple A17 Bionic");
                result.AppendLine("   - RAM: 8GB");
                result.AppendLine("   - Bộ nhớ: 256GB");
            }
            else if (query.Contains("samsung"))
            {
                result.AppendLine("📱 Samsung Galaxy S24 Ultra");
                result.AppendLine("   - Giá: 25.000.000 VND");
                result.AppendLine("   - Tình trạng: Còn 8 cái");
                result.AppendLine("   - CPU: Snapdragon 8 Gen 3");
                result.AppendLine("   - RAM: 12GB");
            }
            else if (query.Contains("laptop"))
            {
                result.AppendLine("💻 Laptop Dell XPS 15");
                result.AppendLine("   - Giá: 38.000.000 VND");
                result.AppendLine("   - Tình trạng: Còn 3 cái");
                result.AppendLine("   - CPU: Intel Core i7 Gen 13");
                result.AppendLine("   - RAM: 32GB");
            }
            else
            {
                result.AppendLine("💬 Vui lòng cho mình biết bạn cần tìm laptop hay điện thoại nhé!");
            }

            result.AppendLine("\n⚠️ Đây là dữ liệu mẫu. Đang kết nối database thực...");
            return result.ToString();
        }

        // HÀM HỖ TRỢ: TRÍCH XUẤT TỪ KHÓA
        private List<string> ExtractKeywords(string message)
        {
            var keywords = new List<string>();
            var brands = new[] { "iphone", "samsung", "oppo", "xiaomi", "realme", "dell", "asus", "hp", "lenovo", "acer", "msi" };

            foreach (var brand in brands)
            {
                if (message.Contains(brand))
                    keywords.Add(brand);
            }

            // Từ khóa đặc biệt
            if (message.Contains("gaming")) keywords.Add("gaming");
            if (message.Contains("cao cấp") || message.Contains("flagship")) keywords.Add("pro");
            if (message.Contains("văn phòng") || message.Contains("office")) keywords.Add("office");

            return keywords;
        }

        // HÀM HỖ TRỢ: TRÍCH XUẤT KHOẢNG GIÁ
        private (decimal? min, decimal? max) ExtractPriceRange(string message)
        {
            decimal? min = null, max = null;

            // Tìm số (triệu, tr, nghìn, k)
            var pricePatterns = new[]
            {
                (@"(\d+)\s*tr", 1000000m),        // "20 tr" = 20 triệu
                (@"(\d+)\s*triệu", 1000000m),     // "20 triệu"
                (@"(\d+)\s*k", 1000m),            // "500k"
                (@"(\d+)\s*nghìn", 1000m)         // "500 nghìn"
            };

            var numbers = new List<decimal>();
            foreach (var (pattern, multiplier) in pricePatterns)
            {
                var matches = System.Text.RegularExpressions.Regex.Matches(message, pattern);
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    if (decimal.TryParse(match.Groups[1].Value, out var value))
                    {
                        numbers.Add(value * multiplier);
                    }
                }
            }

            if (numbers.Any())
            {
                if (message.Contains("dưới") || message.Contains("tối đa"))
                {
                    max = numbers.Max();
                }
                else if (message.Contains("trên") || message.Contains("từ"))
                {
                    min = numbers.Min();
                }
                else if (numbers.Count >= 2)
                {
                    min = numbers.Min();
                    max = numbers.Max();
                }
                else
                {
                    // Nếu chỉ có 1 số, tạo khoảng +/- 20%
                    var price = numbers[0];
                    min = price * 0.8m;
                    max = price * 1.2m;
                }
            }

            return (min, max);
        }
    }

    // ==================== MODELS ====================
    public class AskRequest
    {
        public string? UserPrompt { get; set; }
    }

    public class ChatMessage
    {
        public string UserPrompt { get; set; } = string.Empty;
        public string AiResponse { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}