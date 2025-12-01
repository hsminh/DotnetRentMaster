namespace Payments.MoMo.Models;

public class MoMoConfig
{
    public string PartnerCode { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string NotifyUrl { get; set; } = string.Empty;
    public string ApiEndpoint { get; set; } = "https://test-payment.momo.vn/v2/gateway/api/create";
}
