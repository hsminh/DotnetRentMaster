namespace Payments.VnPay.Models;

public class VnPayTransactionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string TransactionNo { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    public string? BankTranNo { get; set; }
    public string? CardType { get; set; }
    public string? PayDate { get; set; }
    public string? TransactionStatus { get; set; }
    public string? ResponseCode { get; set; }
    public string? TxnResponseCode { get; set; }
    public string? SecureHashType { get; set; }
    public string? SecureHash { get; set; }
}
