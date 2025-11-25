using System.Threading.Tasks;

namespace WEBBANDIENTHOAI.Services
{
    public interface IOTPService
    {
        string GenerateOTP();
        Task<bool> ValidateOTPAsync(string email, string otp);
        Task StoreOTPAsync(string email, string otp);
    }
}