namespace Payments.VnPay.Models;

public class VnPayPaymentResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string PaymentUrl { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
}
