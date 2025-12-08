using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Payments.MoMo.Models;
using Payments.MoMo.Services;
using RentMaster.Core.Models;
using RentMaster.Data;

namespace Management.RealEstate.Controllers.Payment;

[ApiController]
[Route("consumer/api")]
public class PaymentController : ControllerBase
{
    private readonly IMoMoPaymentService _momoService;
    private readonly ILogger<PaymentController> _logger;
    private readonly AppDbContext _context;

    public PaymentController(
        IMoMoPaymentService momoService,
        ILogger<PaymentController> logger,
        AppDbContext context)
    {
        _momoService = momoService;
        _logger = logger;
        _context = context;
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
    public IActionResult ProcessMoMoIPN([FromBody] MoMoIpnModel data)
    {
        try
        {
            _logger.LogInformation("Received IPN from MoMo: {Data}", JsonSerializer.Serialize(data));

            if (data == null)
            {
                _logger.LogWarning("IPN data is null");
                return BadRequest(new { message = "Invalid IPN data" });
            }

            // Verify signature
            // Build raw hash string for verification in the same order as creation
            var rawHash = new StringBuilder();
            rawHash.Append($"accessKey={_momoService.GetAccessKey()}&");  // You'll need to add this getter
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
            _logger.LogDebug("IPN Verification hash: {Hash}", rawHashString);

            // Verify signature
            if (!_momoService.VerifySignature(rawHashString, data.Signature))
            {
                _logger.LogError("IPN signature verification failed for orderId: {OrderId}", data.OrderId);
                return Unauthorized(new { message = "Invalid signature" });
            }

            // Signature is valid, process the payment result
            _logger.LogInformation("IPN signature verified successfully for orderId: {OrderId}, ResultCode: {ResultCode}",
                data.OrderId, data.ResultCode);

            if (data.ResultCode == 0)
            {
                _logger.LogInformation("Payment successful - OrderId: {OrderId}, TransId: {TransId}, Amount: {Amount}",
                    data.OrderId, data.TransId, data.Amount);
                // TODO: Update your database with successful payment
            }
            else
            {
                _logger.LogWarning("Payment failed - OrderId: {OrderId}, ResultCode: {ResultCode}, Message: {Message}",
                    data.OrderId, data.ResultCode, data.Message);
                // TODO: Update your database with failed payment
            }

            return Ok(new { message = "IPN processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MoMo IPN");
            return StatusCode(500, new { message = "An error occurred while processing IPN" });
        }
    }

    [HttpGet("momo/return")]
    public IActionResult MoMoReturnUrl()
    {
        var query = HttpContext.Request.Query;

        string partnerCode = query["partnerCode"];
        string orderId = query["orderId"];
        string requestId = query["requestId"];
        long amount = long.TryParse(query["amount"], out var a) ? a : 0;
        string orderInfo = query["orderInfo"];
        string orderType = query["orderType"];
        long transId = long.TryParse(query["transId"], out var t) ? t : 0;
        int resultCode = int.TryParse(query["resultCode"], out var r) ? r : -1;
        string message = query["message"];
        string payType = query["payType"];
        long responseTime = long.TryParse(query["responseTime"], out var rt) ? rt : 0;
        string extraData = query["extraData"].ToString() ?? string.Empty;
        string signature = query["signature"];
        string redirectUrl = query["redirectUrl"].ToString() ?? "/";

        _logger.LogInformation("Payment return with data: {OrderId}, ResultCode: {ResultCode}", orderId, resultCode);

        if (resultCode == 0)
        {
            try
            {
                // Create and save transaction
                var transaction = new PaymentTransaction
                {
                    OrderId = orderId,
                    PartnerCode = partnerCode,
                    Amount = amount / 100, // Convert to VND
                    Message = message,
                    Status = "Success",
                    PayType = payType
                };

                _context.PaymentTransactions.Add(transaction);
                // await _context.SaveChangesAsync();
                
                _logger.LogInformation("Payment transaction logged for order {OrderId}", orderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging payment transaction for order {OrderId}", orderId);
            }

            return Redirect($"{redirectUrl}?status=success&orderId={orderId}");
        }

        return Redirect($"{redirectUrl}?status=failed&orderId={orderId}&message={message}");
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
