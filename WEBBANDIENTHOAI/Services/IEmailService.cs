using System.Threading.Tasks;

namespace WEBBANDIENTHOAI.Services
{
    public interface IEmailService
    {
        Task<bool> SendOTPEmailAsync(string email, string otp, string customerName);
    }
}