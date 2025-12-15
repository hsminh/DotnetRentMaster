using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Payments.MoMo.Models;
using Payments.MoMo.Services;
using RentMaster.Core.Models;
using RentMaster.Data;
using RentMaster.Management.RentalContract.Services;

namespace Management.RealEstate.Controllers.Payment;

[ApiController]
[Route("consumer/api")]
public class PaymentController : ControllerBase
{
    private readonly IMoMoPaymentService _momoService;
    private readonly ILogger<PaymentController> _logger;
    private readonly AppDbContext _context;
    private readonly RentalContractMonthlyPaymentService _rentalPaymentService;

    public PaymentController(
        IMoMoPaymentService momoService,
        ILogger<PaymentController> logger,
        AppDbContext context,
        RentalContractMonthlyPaymentService rentalPaymentService)
    {
        _momoService = momoService;
        _logger = logger;
        _context = context;
        _rentalPaymentService = rentalPaymentService;
    }

    [HttpPost("momo/payment")]
    public async Task<IActionResult> CreateMoMoPayment([FromBody] MoMoPaymentRequestModel model)
    {
        try
        {
            _logger.LogInformation("Received payment request: {Model}", JsonSerializer.Serialize(model));
            
            // Basic model validation
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                _logger.LogWarning("Invalid request model: {Errors}", string.Join(", ", errors));
                return BadRequest(new { Success = false, Errors = errors });
            }

            // Create payment request
            var request = new MoMoPaymentRequest
            {
                RequestId = _momoService.GenerateRequestId(),
                Amount = model.Amount,
                OrderId = string.IsNullOrWhiteSpace(model.OrderId) ? model.OrderId : model.OrderId.Trim(),
                OrderInfo = string.IsNullOrWhiteSpace(model.OrderInfo) 
                    ? $"Payment for order {model.OrderId}" 
                    : model.OrderInfo,  // Don't trim - keep exact value for signature consistency
                ExtraData = model.ExtraData ?? string.Empty,
                RequestType = "captureWallet",
                IpnUrl = _momoService.GetIpnUrl(),
                RedirectUrl = _momoService.GetReturnUrl(),
                PartnerCode = _momoService.GetPartnerCode()
            };

            // Validate the request
            if (!request.IsValid(out var validationError))
            {
                _logger.LogWarning("Invalid payment request: {Error}", validationError);
                return BadRequest(new { Success = false, Message = validationError });
            }

            _logger.LogInformation("Sending payment request to MoMo: {Request}", 
                JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true }));

            // Process payment
            var response = await _momoService.CreatePaymentAsync(request);
            
            if (response.ResultCode == 0) // Success
            {
                _logger.LogInformation("Payment request successful. PayUrl: {PayUrl}", response.PayUrl);
                
                // Log successful transaction
                _logger.LogInformation("MoMo Payment Success - OrderId: {OrderId}, Amount: {Amount}, RequestId: {RequestId}",
                    request.OrderId, request.Amount, request.RequestId);
                
                return Ok(new
                {
                    Success = true,
                    Data = new
                    {
                        PayUrl = response.PayUrl,
                        Deeplink = response.Deeplink,
                        QrCodeUrl = response.QrCodeUrl,
                        DeeplinkWebInApp = response.DeeplinkWebInApp,
                        RequestId = request.RequestId,
                        OrderId = request.OrderId,
                        Amount = request.Amount,
                        OrderInfo = request.OrderInfo
                    }
                });
            }

            // Log failed payment
            _logger.LogWarning("MoMo Payment Failed - OrderId: {OrderId}, ResultCode: {ResultCode}, Message: {Message}",
                request.OrderId, response.ResultCode, response.Message);
                
            return BadRequest(new 
            { 
                Success = false, 
                ResultCode = response.ResultCode,
                Message = response.Message,
                OrderId = request.OrderId,
                RequestId = request.RequestId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating MoMo payment");
            return StatusCode(500, new { Success = false, Message = "An error occurred while processing your payment" });
        }
    }

    [HttpPost("momo/ipn")]
    public async Task<IActionResult> ProcessMoMoIPN([FromBody] MoMoIpnModel data)
    {
        try
        {
            _logger.LogInformation("Received IPN from MoMo: {Data}", JsonSerializer.Serialize(data));

            if (data == null)
            {
                _logger.LogWarning("IPN data is null");
                return BadRequest(new { message = "Invalid IPN data" });
            }

            _logger.LogInformation("Processing IPN for orderId: {OrderId}, ResultCode: {ResultCode}",
                data.OrderId, data.ResultCode);

            if (data.ResultCode == 0)
            {
                if (Guid.TryParse(data.ExtraData, out var paymentUid))
                {
                    _logger.LogInformation("Payment successful - PaymentUid: {PaymentUid}, OrderId: {OrderId}, Amount: {Amount}, TransId: {TransId}",
                        paymentUid, data.OrderId, data.Amount, data.TransId);

                    var payment = await _rentalPaymentService.MarkAsPaidAsync(paymentUid, null, "MoMo", data.TransId.ToString(), data.RequestId);
                    if (payment != null)
                    {
                        _logger.LogInformation("Monthly payment marked as paid. PaymentUid: {PaymentUid}, TransId: {TransId}", paymentUid, data.TransId);
                        return Ok(new { message = "IPN processed successfully" });
                    }
                    else
                    {
                        _logger.LogWarning("Failed to mark payment as paid. PaymentUid: {PaymentUid}", paymentUid);
                        return BadRequest(new { message = "Failed to mark payment as paid" });
                    }
                }
                else
                {
                    _logger.LogError("Invalid ExtraData format: {ExtraData}", data.ExtraData);
                    return BadRequest(new { message = "Invalid ExtraData format" });
                }
            }
            else
            {
                _logger.LogWarning("Payment failed - OrderId: {OrderId}, ResultCode: {ResultCode}, Message: {Message}",
                    data.OrderId, data.ResultCode, data.Message);
                return Ok(new { message = "Payment failed but IPN processed" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MoMo IPN");
            return StatusCode(500, new { message = "An error occurred while processing IPN" });
        }
    }


}

public class MoMoPaymentRequestModel
{
    public long Amount { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public string OrderInfo { get; set; } = string.Empty;
    public string ExtraData { get; set; } = string.Empty;
}

public class MoMoReturnModel
{
    public string PartnerCode { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public long Amount { get; set; }
    public string OrderInfo { get; set; } = string.Empty;
    public string OrderType { get; set; } = string.Empty;
    public long TransId { get; set; }
    public int ResultCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string PayType { get; set; } = string.Empty;
    public long ResponseTime { get; set; }
    public string? ExtraData { get; set; } 
    public string Signature { get; set; } = string.Empty;
    public string RedirectUrl { get; set; } = string.Empty;
}
