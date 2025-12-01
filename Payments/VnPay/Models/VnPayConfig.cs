namespace Payments.VnPay.Models;

public class VnPayConfig
{
    public string Version { get; set; } = "2.1.0";
    public string TmnCode { get; set; } = string.Empty;
    public string HashSecret { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://sandbox.vnpayment.vn";
    public string ReturnUrl { get; set; } = "/payment/vnpay-return";
    public string IpnUrl { get; set; } = "/payment/vnpay-ipn";
    public string Command { get; set; } = "pay";
    public string OrderType { get; set; } = "other";
    public string Locale { get; set; } = "vn";
    public string CurrencyCode { get; set; } = "VND";
    public string PaymentUrl { get; set; } = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
    public string RefundUrl { get; set; } = "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction";
    public string QueryUrl { get; set; } = "https://sandbox.vnpayment.vn/merchant_webapi/api/transaction";
}
