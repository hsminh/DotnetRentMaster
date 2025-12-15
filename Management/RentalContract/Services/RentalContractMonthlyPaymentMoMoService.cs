using System.Text;
using Microsoft.EntityFrameworkCore;
using Payments.MoMo.Models;
using Payments.MoMo.Services;
using RentMaster.Data;
using RentMaster.Management.RentalContract.Models;

namespace RentMaster.Management.RentalContract.Services;

public class RentalContractMonthlyPaymentMoMoService
{
    private readonly IMoMoPaymentService _momoService;
    private readonly RentalContractMonthlyPaymentService _paymentService;
    private readonly AppDbContext _context;
    private readonly ILogger<RentalContractMonthlyPaymentMoMoService> _logger;

    public RentalContractMonthlyPaymentMoMoService(
        IMoMoPaymentService momoService,
        RentalContractMonthlyPaymentService paymentService,
        AppDbContext context,
        ILogger<RentalContractMonthlyPaymentMoMoService> logger)
    {
        _momoService = momoService;
        _paymentService = paymentService;
        _context = context;
        _logger = logger;
    }

    public async Task<(bool Success, string? PayUrl, string? Message, string? RequestId, string? OrderId)> 
        CreatePaymentRequestAsync(Guid monthlyPaymentUid)
    {
        try
        {
            var payment = await _context.RentalContractMonthlyPayments
                .Include(p => p.RentalContract)
                .FirstOrDefaultAsync(p => p.Uid == monthlyPaymentUid && !p.IsDelete);

            if (payment == null)
                return (false, null, "Payment not found", null, null);

            if (payment.IsPaid)
                return (false, null, "Payment is already paid", null, null);

            var orderId = $"MONTHLY-{payment.RentalContractUid:N}-{payment.Year}-{payment.Month}";
            var orderInfo = $"Thanh toán hóa đơn tháng {payment.Month}/{payment.Year} - Hợp đồng {payment.RentalContractUid:N}";

            var request = new MoMoPaymentRequest
            {
                RequestId = _momoService.GenerateRequestId(),
                Amount = (long)(payment.Amount * 100),
                OrderId = orderId,
                OrderInfo = orderInfo,
                ExtraData = monthlyPaymentUid.ToString("N"),
                RequestType = "captureWallet",
                IpnUrl = _momoService.GetIpnUrl(),
                RedirectUrl = _momoService.GetReturnUrl(),
                PartnerCode = _momoService.GetPartnerCode()
            };

            if (!request.IsValid(out var validationError))
            {
                _logger.LogWarning("Invalid payment request: {Error}", validationError);
                return (false, null, validationError, null, null);
            }

            _logger.LogInformation("Creating MoMo payment for monthly payment {PaymentUid}, OrderId: {OrderId}",
                monthlyPaymentUid, orderId);

            var response = await _momoService.CreatePaymentAsync(request);

            if (response.ResultCode == 0)
            {
                _logger.LogInformation("MoMo payment created successfully. PayUrl: {PayUrl}", response.PayUrl);
                return (true, response.PayUrl, "Success", request.RequestId, orderId);
            }

            _logger.LogWarning("MoMo payment creation failed. ResultCode: {ResultCode}, Message: {Message}",
                response.ResultCode, response.Message);
            return (false, null, response.Message, null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating MoMo payment for monthly payment {PaymentUid}",
                monthlyPaymentUid);
            return (false, null, "An error occurred while processing payment", null, null);
        }
    }

    public async Task<bool> HandleMoMoIpnAsync(MoMoIpnModel data)
    {
        try
        {
            if (data == null)
            {
                _logger.LogWarning("IPN data is null");
                return false;
            }

            _logger.LogInformation("Processing MoMo IPN for OrderId: {OrderId}, ResultCode: {ResultCode}",
                data.OrderId, data.ResultCode);

            var rawHash = new StringBuilder();
            rawHash.Append($"accessKey={_momoService.GetAccessKey()}&");
            rawHash.Append($"amount={data.Amount}&");
            rawHash.Append($"extraData={data.ExtraData}&");
            rawHash.Append($"ipnUrl={_momoService.GetIpnUrl()}&");
            rawHash.Append($"orderId={data.OrderId}&");
            rawHash.Append($"orderInfo={Uri.EscapeDataString(data.OrderInfo)}&");
            rawHash.Append($"partnerCode={data.PartnerCode}&");
            rawHash.Append($"redirectUrl={_momoService.GetReturnUrl()}&");
            rawHash.Append($"requestId={data.RequestId}&");
            rawHash.Append($"requestType=captureWallet&");
            rawHash.Append($"responseTime={data.ResponseTime}&");
            rawHash.Append($"resultCode={data.ResultCode}&");
            rawHash.Append($"transId={data.TransId}");

            var rawHashString = rawHash.ToString();

            if (!_momoService.VerifySignature(rawHashString, data.Signature))
            {
                _logger.LogError("IPN signature verification failed for orderId: {OrderId}", data.OrderId);
                return false;
            }

            _logger.LogInformation("IPN signature verified successfully for orderId: {OrderId}", data.OrderId);

            if (data.ResultCode == 0)
            {
                if (Guid.TryParse(data.ExtraData, out var paymentUid))
                {
                    _logger.LogInformation("Payment successful - PaymentUid: {PaymentUid}, OrderId: {OrderId}, Amount: {Amount}",
                        paymentUid, data.OrderId, data.Amount);

                    var payment = await _paymentService.MarkAsPaidAsync(paymentUid, null, "MoMo");
                    if (payment != null)
                    {
                        _logger.LogInformation("Monthly payment marked as paid. PaymentUid: {PaymentUid}", paymentUid);
                        return true;
                    }
                    else
                    {
                        _logger.LogWarning("Failed to mark payment as paid. PaymentUid: {PaymentUid}", paymentUid);
                        return false;
                    }
                }
                else
                {
                    _logger.LogError("Invalid ExtraData format: {ExtraData}", data.ExtraData);
                    return false;
                }
            }
            else
            {
                _logger.LogWarning("Payment failed - OrderId: {OrderId}, ResultCode: {ResultCode}",
                    data.OrderId, data.ResultCode);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MoMo IPN");
            return false;
        }
    }

    public async Task<bool> HandleMoMoReturnAsync(string orderId, int resultCode, string extraData)
    {
        try
        {
            _logger.LogInformation("Processing MoMo return - OrderId: {OrderId}, ResultCode: {ResultCode}",
                orderId, resultCode);

            if (resultCode == 0 && Guid.TryParse(extraData, out var paymentUid))
            {
                var payment = await _paymentService.GetPaymentAsync(paymentUid);
                if (payment != null && payment.IsPaid)
                {
                    _logger.LogInformation("Payment already marked as paid via IPN. PaymentUid: {PaymentUid}",
                        paymentUid);
                    return true;
                }

                _logger.LogInformation("Payment returned successfully but not yet marked as paid. PaymentUid: {PaymentUid}",
                    paymentUid);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MoMo return");
            return false;
        }
    }
}
