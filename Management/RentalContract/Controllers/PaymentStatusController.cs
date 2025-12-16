using Microsoft.AspNetCore.Mvc;
using RentMaster.Management.RentalContract.Services;

namespace RentMaster.Management.RentalContract.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentStatusController : ControllerBase
{
    private readonly RentalContractMonthlyPaymentService _paymentService;
    private readonly ILogger<PaymentStatusController> _logger;

    public PaymentStatusController(
        RentalContractMonthlyPaymentService paymentService,
        ILogger<PaymentStatusController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpGet("{orderId}/status")]
    public async Task<IActionResult> GetPaymentStatus(string orderId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(orderId))
                return BadRequest(new { status = "invalid", message = "OrderId is required" });

            _logger.LogInformation("Checking payment status for orderId: {OrderId}", orderId);

            var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);

            if (payment == null)
            {
                _logger.LogWarning("Payment not found for orderId: {OrderId}", orderId);
                return NotFound(new { status = "pending", message = "Payment not found or still processing" });
            }

            var status = payment.IsPaid ? "success" : "pending";

            _logger.LogInformation("Payment status for orderId {OrderId}: {Status}", orderId, status);

            return Ok(new
            {
                status = status,
                amount = payment.Amount,
                paidAt = payment.PaidAt,
                message = payment.IsPaid ? "Thanh toán thành công!" : "Chờ xác nhận thanh toán..."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking payment status for orderId: {OrderId}", orderId);
            return StatusCode(500, new { status = "error", message = "An error occurred while checking payment status" });
        }
    }
}
