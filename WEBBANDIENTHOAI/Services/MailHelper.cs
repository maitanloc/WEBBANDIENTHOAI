using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace WEBBANDIENTHOAI.Services
{
    public interface IMailService
    {
        Task<bool> SendOTPEmailAsync(string email, string otp, string customerName);
        Task<bool> SendPasswordResetSuccessAsync(string email, string customerName);
    }

    public class MailService : IMailService
    {
        private readonly IConfiguration _configuration;

        public MailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendOTPEmailAsync(string email, string otp, string customerName)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var port = int.Parse(_configuration["EmailSettings:Port"]);
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderPassword = _configuration["EmailSettings:SenderPassword"];
                var senderName = _configuration["EmailSettings:SenderName"];

                using (var smtpClient = new SmtpClient(smtpServer, port))
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    smtpClient.EnableSsl = true;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.Timeout = 30000;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail, senderName),
                        Subject = "🔐 Mã OTP Đặt Lại Mật Khẩu - FOXMOBILE",
                        Body = GenerateOTPEmailBody(customerName, otp),
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(email);

                    await smtpClient.SendMailAsync(mailMessage);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gửi email OTP: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetSuccessAsync(string email, string customerName)
        {
            try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var port = int.Parse(_configuration["EmailSettings:Port"]);
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var senderPassword = _configuration["EmailSettings:SenderPassword"];
                var senderName = _configuration["EmailSettings:SenderName"];

                using (var smtpClient = new SmtpClient(smtpServer, port))
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    smtpClient.EnableSsl = true;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.Timeout = 30000;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail, senderName),
                        Subject = "✅ Đặt Lại Mật Khẩu Thành Công - FOXMOBILE",
                        Body = GenerateSuccessEmailBody(customerName),
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(email);

                    await smtpClient.SendMailAsync(mailMessage);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gửi email thành công: {ex.Message}");
                return false;
            }
        }

        private string GenerateOTPEmailBody(string customerName, string otp)
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

        private string GenerateSuccessEmailBody(string customerName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: 'Inter', Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #22c55e, #16a34a); padding: 30px; text-align: center; color: white; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f8f9fa; padding: 30px; border-radius: 0 0 10px 10px; }}
        .success-icon {{ font-size: 48px; text-align: center; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #666; font-size: 14px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🦊 FOXMOBILE</h1>
            <p>Thế giới công nghệ cao cấp</p>
        </div>
        
        <div class='content'>
            <div class='success-icon'>✅</div>
            <h2>Mật khẩu đã được đặt lại thành công!</h2>
            
            <p>Xin chào <strong>{customerName}</strong>,</p>
            
            <p>Mật khẩu cho tài khoản FOXMOBILE của bạn đã được thay đổi thành công.</p>
            
            <div style='background: #d1fae5; border: 1px solid #a7f3d0; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                <strong>🔒 Thông tin bảo mật:</strong>
                <ul>
                    <li>Mật khẩu mới đã được mã hóa an toàn</li>
                    <li>Bạn có thể sử dụng mật khẩu mới để đăng nhập ngay</li>
                    <li>Nếu bạn không thực hiện thay đổi này, vui lòng liên hệ hỗ trợ ngay lập tức</li>
                </ul>
            </div>
            
            <p><a href='{_configuration["AppSettings:BaseUrl"]}/Account/Login' style='background: #22c55e; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px; display: inline-block;'>Đăng nhập ngay</a></p>
            
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