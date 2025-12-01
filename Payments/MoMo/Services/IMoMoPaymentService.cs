using Payments.MoMo.Models;

namespace Payments.MoMo.Services;

public interface IMoMoPaymentService
{
    Task<MoMoPaymentResponse> CreatePaymentAsync(MoMoPaymentRequest request);
    string GenerateSignature(string text);
    bool VerifySignature(string text, string signature);
    string GenerateRequestId();
    string GetIpnUrl();
    string GetReturnUrl();
    string GetPartnerCode();
    string GetAccessKey();
}
