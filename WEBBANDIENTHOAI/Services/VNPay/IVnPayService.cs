using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using WEBBANDIENTHOAI.Models;

namespace WEBBANDIENTHOAI.Services.VNPay
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}
