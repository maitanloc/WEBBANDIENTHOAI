using Microsoft.AspNetCore.Mvc;
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
// Giả định các Models Sản phẩm và Cấu hình nằm trong các namespace này
// Nếu báo lỗi, bạn cần kiểm tra lại chính xác namespace của các Models kia.
// using WebBanDienThoai.Models; 

namespace WEBBANDIENTHOAI.Controllers
{
    public class ChatController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiKey;
        // Dùng mô hình Flash cho tốc độ xử lý nhanh
        private const string ApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

        // --- ĐỊNH NGHĨA NHÂN CÁCH FOX AI (System Instruction) ---
        private const string SystemInstruction = @"
            Bạn tên là Fox AI, chuyên gia tư vấn bán hàng điện thoại và laptop của FOXMOBILE.
            1. Trả lời thân thiện, nhiệt tình, xưng hô 'Fox AI' và gọi khách là 'bạn'.
            2. Cần phải phân tích nhu cầu khách hàng (CPU, RAM, giá) và đưa ra gợi ý sản phẩm tốt nhất dựa trên dữ liệu.
            3. Khi tư vấn, hãy so sánh cấu hình, giá và TỒN KHO.
            4. Luôn mời khách hàng đặt hàng hoặc để lại thông tin liên hệ khi chốt được sản phẩm.
            5. Tuyệt đối không bịa đặt thông tin. Nếu không có dữ liệu, hãy trả lời theo chính sách chung.
        ";

        public ChatController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _apiKey = _configuration.GetValue<string>("AppSettings:Gemini:ApiKey") ?? string.Empty;
        }

        // Action mặc định để hiển thị view chat (nếu có View riêng)
        public IActionResult Index()
        {
            return View();
        }

        // Action chính để nhận tin nhắn và trả lời
        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] AskRequest request)
        {
            if (string.IsNullOrEmpty(request.UserPrompt) || string.IsNullOrEmpty(_apiKey))
            {
                return Json(new { success = false, message = "Lỗi: Vui lòng nhập câu hỏi hoặc kiểm tra API Key." });
            }

            try
            {
                // BƯỚC 1: LẤY LỊCH SỬ CHAT (TRÍ NHỚ)
                var historyJson = HttpContext.Session.GetString("ChatHistory") ?? JsonSerializer.Serialize(new List<ChatMessage>());
                var history = JsonSerializer.Deserialize<List<ChatMessage>>(historyJson)!;

                var contents = new List<object>();
                // Thêm lịch sử vào request
                foreach (var message in history)
                {
                    contents.Add(new { role = "user", parts = new[] { new { text = message.UserPrompt } } });
                    contents.Add(new { role = "model", parts = new[] { new { text = message.AiResponse } } });
                }

                // BƯỚC 2: GẮN DỮ LIỆU TƯ VẤN (RAG) - TẠM THỜI VÔ HIỆU HÓA ĐỂ DEBUG
                string thongTinBoSung = ""; // await GetConsultingContext(request.UserPrompt!);
                var finalUserPrompt = $"Dữ liệu tư vấn: [{thongTinBoSung}]. Câu hỏi của khách hàng: {request.UserPrompt}. Hãy trả lời khách hàng dựa trên dữ liệu này.";

                // Thêm câu hỏi mới của khách hàng
                contents.Add(new { role = "user", parts = new[] { new { text = finalUserPrompt } } });

                // BƯỚC 3: TẠO REQUEST BODY HOÀN CHỈNH
                var requestBody = new
                {
                    system_instruction = new
                    {
                        parts = new[] { new { text = SystemInstruction } }
                    },
                    contents = contents.ToArray()
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                // Gửi request POST
                var response = await _httpClient.PostAsync(ApiUrl + "?key=" + _apiKey, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    // Phân tích phản hồi JSON
                    using (JsonDocument doc = JsonDocument.Parse(responseString))
                    {
                        var text = doc.RootElement
                            .GetProperty("candidates")[0]
                            .GetProperty("content")
                            .GetProperty("parts")[0]
                            .GetProperty("text")
                            .GetString();

                        // BƯỚC 4: LƯU LỊCH SỬ MỚI VÀO SESSION
                        history.Add(new ChatMessage { UserPrompt = request.UserPrompt!, AiResponse = text!, Timestamp = DateTime.Now });
                        HttpContext.Session.SetString("ChatHistory", JsonSerializer.Serialize(history.TakeLast(20).ToList()));

                        return Json(new { success = true, aiResponse = text });
                    }
                }

                return Json(new { success = false, message = $"Lỗi API: {response.StatusCode}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi hệ thống: {ex.Message}" });
            }
        }

        // Action để tải lịch sử (dùng cho JS)
        public IActionResult GetChatHistory()
        {
            var historyJson = HttpContext.Session.GetString("ChatHistory");
            var history = string.IsNullOrEmpty(historyJson)
                ? new List<ChatMessage>()
                : JsonSerializer.Deserialize<List<ChatMessage>>(historyJson);

            return Json(history);
        }

        // Action để xóa lịch sử
        [HttpPost]
        public IActionResult ClearHistory()
        {
            HttpContext.Session.Remove("ChatHistory");
            return Json(new { success = true });
        }

        // --- HÀM TƯ VẤN SẢN PHẨM VÀ KHO HÀNG (Mô phỏng Truy vấn DB) ---
        private string GetConsultingContext(string userMessage)
        {
            // Trong thực tế: Dùng AppDbContext để truy vấn các Models Product, Inventory, Configuration
            // Dưới đây là dữ liệu cứng giả lập để test Fox AI ngay lập tức

            var result = new StringBuilder();
            userMessage = userMessage.ToLower();

            if (userMessage.Contains("laptop") || userMessage.Contains("ram 32gb"))
            {
                result.AppendLine("Sản phẩm: Laptop Dell XPS 15. Giá: 38.000.000 VND. Tóm tắt: Thiết kế mỏng, màn OLED. Tồn kho: 3 cái.");
                result.AppendLine("Cấu hình chi tiết: Chip Intel Core i7 Gen 13, RAM 32GB, Ổ cứng 1TB SSD, Màn hình 15.6 inch.");
            }
            if (userMessage.Contains("samsung") || userMessage.Contains("điện thoại cao cấp"))
            {
                result.AppendLine("Sản phẩm: Samsung Galaxy S24 Ultra. Giá: 25.000.000 VND. Tóm tắt: Camera 200MP, S Pen tích hợp. Tồn kho: 12 cái.");
                result.AppendLine("Cấu hình chi tiết: Chip Snapdragon 8 Gen 3, RAM 12GB, Bộ nhớ 512GB, Pin 5000mAh.");
            }
            if (userMessage.Contains("iphone"))
            {
                result.AppendLine("Sản phẩm: iPhone 15 Pro Max. Giá: 28.000.000 VND. Tóm tắt: Chip A17 Bionic, vỏ Titan. Tồn kho: 0 cái.");
            }

            if (result.Length == 0)
            {
                return "Chính sách FOXMOBILE: Bảo hành 1 năm, Ship COD toàn quốc 30k. Khuyến mãi: Giảm 5% cho đơn hàng trên 50 triệu.";
            }

            return result.ToString();
        }
    }
}