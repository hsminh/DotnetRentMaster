namespace Payments.VnPay.Models;

public class VnPayPaymentResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string BankCode { get; set; } = string.Empty;
    public string BankTranNo { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;
    public string ResponseCode { get; set; } = string.Empty;
    public string TransactionStatus { get; set; } = string.Empty;
    public DateTime? PayDate { get; set; }
}
