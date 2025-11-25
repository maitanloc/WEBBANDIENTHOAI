namespace WEBBANDIENTHOAI.Services
{
    public interface ISMSService
    {
        Task<bool> SendOTPSMSAsync(string phone, string otp);
    }
}
