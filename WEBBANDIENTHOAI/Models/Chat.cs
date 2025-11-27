using System;

namespace WEBBANDIENTHOAI.Models
{
    // Model để hứng request từ Ajax
    public class AskRequest
    {
        public string? UserPrompt { get; set; }
    }

    // Model cho lịch sử chat, lưu trong Session
    public class ChatMessage
    {
        public string UserPrompt { get; set; } = string.Empty;
        public string AiResponse { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    // Giữ lại class Chat (dù không dùng trực tiếp trong ChatController)
    public class Chat
    {
        // Có thể để trống hoặc dùng cho các mục đích khác
    }
}