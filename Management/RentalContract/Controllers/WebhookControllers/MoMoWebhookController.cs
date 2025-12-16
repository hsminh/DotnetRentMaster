using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Payments.MoMo.Models;
using RentMaster.Management.RentalContract.Services;

namespace RentMaster.Management.RentalContract.Controllers.WebhookControllers;

[ApiController]
[Route("api/webhooks/momo")]
public class MoMoWebhookController : ControllerBase
{
    private readonly RentalContractMonthlyPaymentMoMoService _momoService;
    private readonly ILogger<MoMoWebhookController> _logger;

    public MoMoWebhookController(
        RentalContractMonthlyPaymentMoMoService momoService,
        ILogger<MoMoWebhookController> logger)
    {
        _momoService = momoService;
        _logger = logger;
    }

    [HttpPost("ipn")]
    public async Task<IActionResult> MomoIpn([FromBody] MoMoIpnModel? data)
    {
        try
        {
            _logger.LogInformation("IPN endpoint called - raw data received");

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
}
