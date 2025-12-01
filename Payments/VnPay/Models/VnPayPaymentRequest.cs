using System.ComponentModel.DataAnnotations;

namespace Payments.VnPay.Models;

public class VnPayPaymentRequest
{
    [Required]
    public string OrderId { get; set; } = string.Empty;
    
    [Required]
    [Range(1000, double.MaxValue, ErrorMessage = "Amount must be at least 1,000 VND")]
    public decimal Amount { get; set; }
    
    [Required]
    public string OrderInfo { get; set; } = string.Empty;
    
    public string OrderType { get; set; } = "other";
    public string BankCode { get; set; } = string.Empty;
    public string Language { get; set; } = "vn";
    public string IpAddress { get; set; } = string.Empty;
    public DateTime? CreateDate { get; set; }
    public DateTime? ExpireDate { get; set; }
    public string ReturnUrl { get; set; } = string.Empty;
    public string TmnCode { get; set; } = string.Empty;
}
