using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Payments.MoMo.Models;
using RentMaster.Management.RentalContract.Services;

namespace RentMaster.Management.RentalContract.Controllers;

[ApiController]
[Route("consumer/api/rental-payments")]
public class RentalContractMonthlyPaymentPaymentController : ControllerBase
{
    private readonly RentalContractMonthlyPaymentMoMoService _momoService;
    private readonly ILogger<RentalContractMonthlyPaymentPaymentController> _logger;

    public RentalContractMonthlyPaymentPaymentController(
        RentalContractMonthlyPaymentMoMoService momoService,
        ILogger<RentalContractMonthlyPaymentPaymentController> logger)
    {
        _momoService = momoService;
        _logger = logger;
    }

    [HttpPost("momo/create/{paymentUid:guid}")]
    public async Task<IActionResult> CreateMoMoPaymentRequest(Guid paymentUid)
    {
        try
        {
            _logger.LogInformation("Creating MoMo payment request for payment {PaymentUid}", paymentUid);

            var (success, payUrl, message, requestId, orderId) = 
                await _momoService.CreatePaymentRequestAsync(paymentUid);

            if (!success)
            {
                _logger.LogWarning("Failed to create MoMo payment: {Message}", message);
                return BadRequest(new
                {
                    success = false,
                    message = message,
                    paymentUid = paymentUid
                });
            }

            _logger.LogInformation("MoMo payment created successfully for payment {PaymentUid}", paymentUid);

            return Ok(new
            {
                success = true,
                message = "Payment request created successfully",
                data = new
                {
                    payUrl = payUrl,
                    requestId = requestId,
                    orderId = orderId,
                    paymentUid = paymentUid
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating MoMo payment for payment {PaymentUid}", paymentUid);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while creating payment request"
            });
        }
    }

    [HttpPost("momo/ipn")]
    public async Task<IActionResult> HandleMoMoIPN([FromBody] MoMoIpnModel data)
    {
        try
        {
            _logger.LogInformation("Received MoMo IPN: {Data}", JsonSerializer.Serialize(data));

            if (data == null)
            {
                _logger.LogWarning("IPN data is null");
                return BadRequest(new { message = "Invalid IPN data" });
            }

            var result = await _momoService.HandleMoMoIpnAsync(data);

            if (result)
            {
                _logger.LogInformation("IPN processed successfully for orderId: {OrderId}", data.OrderId);
                return Ok(new { message = "IPN processed successfully" });
            }

            _logger.LogWarning("IPN processing failed for orderId: {OrderId}", data.OrderId);
            return BadRequest(new { message = "IPN processing failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MoMo IPN");
            return StatusCode(500, new { message = "An error occurred while processing IPN" });
        }
    }

    [HttpGet("momo/return")]
    public async Task<IActionResult> HandleMoMoReturn()
    {
        try
        {
            var query = HttpContext.Request.Query;

            string orderId = query["orderId"];
            int resultCode = int.TryParse(query["resultCode"], out var r) ? r : -1;
            string extraData = query["extraData"].ToString() ?? string.Empty;
            string redirectUrl = query["redirectUrl"].ToString() ?? "/";

            _logger.LogInformation("MoMo return with data: OrderId: {OrderId}, ResultCode: {ResultCode}", 
                orderId, resultCode);

            var result = await _momoService.HandleMoMoReturnAsync(orderId, resultCode, extraData);

            if (resultCode == 0)
            {
                _logger.LogInformation("Payment successful for order {OrderId}", orderId);
                return Redirect($"{redirectUrl}?status=success&orderId={orderId}&paymentUid={extraData}");
            }

            _logger.LogWarning("Payment failed for order {OrderId}, ResultCode: {ResultCode}", 
                orderId, resultCode);
            string message = query["message"].ToString() ?? "Payment failed";
            return Redirect($"{redirectUrl}?status=failed&orderId={orderId}&message={message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling MoMo return");
            return StatusCode(500, new { message = "An error occurred while processing payment return" });
        }
    }
}
