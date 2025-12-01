namespace Payments.VnPay.Models;

public class VnPayRefundRequest
{
    public string TransactionId { get; set; } = string.Empty;
    public string VnpTransactionNo { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string RequestBy { get; set; } = string.Empty;
}
