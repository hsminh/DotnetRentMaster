using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Payments.MoMo.Models;

public class MoMoPaymentRequest
{
    [Required]
    [JsonPropertyName("partnerCode")]
    public string PartnerCode { get; set; } = string.Empty;
    
    [Required]
    [JsonPropertyName("requestId")]
    public string RequestId { get; set; } = string.Empty;
    
    [Required]
    [Range(1000, long.MaxValue, ErrorMessage = "Amount must be at least 1,000 VND")]
    [JsonPropertyName("amount")]
    public long Amount { get; set; }
    
    [Required]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "OrderId must be between 1 and 50 characters")]
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255, ErrorMessage = "OrderInfo cannot exceed 255 characters")]
    [JsonPropertyName("orderInfo")]
    public string OrderInfo { get; set; } = string.Empty;
    
    [Required]
    [Url(ErrorMessage = "Invalid RedirectUrl format")]
    [JsonPropertyName("redirectUrl")]
    public string RedirectUrl { get; set; } = string.Empty;
    
    [Required]
    [Url(ErrorMessage = "Invalid IpnUrl format")]
    [JsonPropertyName("ipnUrl")]
    public string IpnUrl { get; set; } = string.Empty;
    
    [Required]
    [RegularExpression("^captureWallet$", ErrorMessage = "RequestType must be 'captureWallet'")]
    [JsonPropertyName("requestType")]
    public string RequestType { get; set; } = "captureWallet";
    
    [StringLength(512, ErrorMessage = "ExtraData cannot exceed 512 characters")]
    [JsonPropertyName("extraData")]
    public string ExtraData { get; set; } = "";
    
    [RegularExpression("^vi|en$", ErrorMessage = "Lang must be either 'vi' or 'en'")]
    [JsonPropertyName("lang")]
    public string Lang { get; set; } = "vi";
    
    [Required]
    [JsonPropertyName("signature")]
    public string Signature { get; set; } = string.Empty;
    
    // Helper method to validate the request
    public bool IsValid(out string errorMessage)
    {
        if (Amount < 1000)
        {
            errorMessage = "Amount must be at least 1,000 VND";
            return false;
        }
        
        if (string.IsNullOrWhiteSpace(OrderId))
        {
            errorMessage = "OrderId is required";
            return false;
        }
        
        if (string.IsNullOrWhiteSpace(OrderInfo))
        {
            errorMessage = "OrderInfo is required";
            return false;
        }
        
        errorMessage = string.Empty;
        return true;
    }
}
