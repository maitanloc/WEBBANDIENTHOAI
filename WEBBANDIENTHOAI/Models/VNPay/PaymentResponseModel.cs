namespace WEBBANDIENTHOAI.Models
{
    public class PaymentResponseModel
    {
        public string OrderDescription { get; set; } // nôin dung 
        public string TransactionId { get; set; } // mã do VNpay tạo ra dùng để tham chiếu
        public string OrderId { get; set; } 
        public string PaymentMethod { get; set; } // phương thức thanh toán 
        public string PaymentId { get; set; }
        public bool Success { get; set; }
        public string Token { get; set; }
        public string VnPayResponseCode { get; set; }// mã phản hồi từ vnpay để biết có giao dịch thành công hay không 

    }
}