using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace WEBBANDIENTHOAI.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendOTPEmailAsync(string email, string otp, string customerName)
        {
            try
            {
                Console.WriteLine($"=== EMAIL SENDING STARTED ===");

                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var port = int.Parse(_configuration["EmailSettings:Port"]);
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderPassword = _configuration["EmailSettings:SenderPassword"];
                var senderName = _configuration["EmailSettings:SenderName"];

                Console.WriteLine($"SMTP Server: {smtpServer}");
                Console.WriteLine($"Port: {port}");
                Console.WriteLine($"Sender Email: {senderEmail}");
                Console.WriteLine($"Sender Password length: {senderPassword?.Length}");
                Console.WriteLine($"Sender Name: {senderName}");
                Console.WriteLine($"Recipient: {email}");
                Console.WriteLine($"OTP: {otp}");

                using (var smtpClient = new SmtpClient(smtpServer, port))
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    smtpClient.EnableSsl = true;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

                    // Thêm timeout để tránh treo
                    smtpClient.Timeout = 30000;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail, senderName),
                        Subject = "🔐 Mã OTP Đặt Lại Mật Khẩu - FOXMOBILE",
                        Body = GenerateEmailBody(customerName, otp),
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(email);

                    Console.WriteLine("Attempting to send email...");
                    await smtpClient.SendMailAsync(mailMessage);
                    Console.WriteLine("Email sent successfully!");

                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== EMAIL SENDING ERROR ===");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Console.WriteLine($"=== END EMAIL ERROR ===");

                return false;
            }
        }

        private string GenerateEmailBody(string customerName, string otp)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Inter', Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #f48c25, #f59e0b); padding: 30px; text-align: center; color: white; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; }}
        .otp-code {{ font-size: 32px; font-weight: bold; color: #f48c25; text-align: center; letter-spacing: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 14px; }}
        .warning {{ background: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🦊 FOXMOBILE</h1>
            <p>Thế giới công nghệ cao cấp</p>
        </div>
        
        <div class='content'>
            <h2>Xin chào {customerName},</h2>
            
            <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản FOXMOBILE.</p>
            
            <div class='otp-code'>{otp}</div>
            
            <div class='warning'>
                <strong>⚠️ Lưu ý quan trọng:</strong>
                <ul>
                    <li>Mã OTP có hiệu lực trong 10 phút</li>
                    <li>Không chia sẻ mã OTP với bất kỳ ai</li>
                    <li>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này</li>
                </ul>
            </div>
            
            <p>Để hoàn tất việc đặt lại mật khẩu, vui lòng:</p>
            <ol>
                <li>Quay lại trang FOXMOBILE</li>
                <li>Nhập mã OTP trên</li>
                <li>Đặt mật khẩu mới cho tài khoản</li>
            </ol>
            
            <p>Nếu bạn cần hỗ trợ, vui lòng liên hệ:</p>
            <ul>
                <li>📞 Hotline: 1800 1234</li>
                <li>📧 Email: support@foxmobile.com</li>
            </ul>
        </div>
        
        <div class='footer'>
            <p>© 2024 FOXMOBILE. Tất cả các quyền được bảo lưu.</p>
            <p>Đây là email tự động, vui lòng không trả lời.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}