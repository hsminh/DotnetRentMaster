using Payments.VnPay.Models;

namespace Payments.VnPay.Services;

public interface IVnPayService
{
    /// <summary>
    /// Create a payment URL for VNPay
    /// </summary>
    Task<VnPayPaymentResponse> CreatePaymentUrlAsync(VnPayPaymentRequest request);
    
    /// <summary>
    /// Process payment return from VNPay
    /// </summary>
    Task<VnPayPaymentResult> ProcessPaymentReturnAsync(Dictionary<string, string> parameters);
    
    /// <summary>
    /// Process IPN (Instant Payment Notification) from VNPay
    /// </summary>
    Task<VnPayIpnResponse> ProcessIpnAsync(Dictionary<string, string> parameters);
    
    /// <summary>
    /// Process refund for a transaction
    /// </summary>
    Task<VnPayRefundResponse> ProcessRefundAsync(VnPayRefundRequest request);
    
    /// <summary>
    /// Query transaction status
    /// </summary>
    Task<VnPayTransactionResponse> QueryTransactionAsync(string transactionId, DateTime transactionDate);
}
