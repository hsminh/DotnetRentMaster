namespace Payments.VnPay.Models;

public class VnPayIpnResponse
{
    public string RspCode { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
