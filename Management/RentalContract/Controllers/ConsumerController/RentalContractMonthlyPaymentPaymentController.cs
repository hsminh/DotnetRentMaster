using Microsoft.AspNetCore.Mvc;
using RentMaster.Management.RentalContract.Services;
using RentMaster.Core.Middleware;

namespace RentMaster.Management.RentalContract.Controllers.ConsumerController;

[ApiController]
[Attributes.UserScope]
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

    [HttpPost("momo/create/{contractUid:guid}")]
    public async Task<IActionResult> CreateMoMoPaymentRequest(Guid contractUid)
    {
        try
        {
            var consumer = HttpContext.GetCurrentUser<Accounts.Models.Consumer>();
            _logger.LogInformation("Creating MoMo payment request for contract {ContractUid}", contractUid);
            
            var (success, payUrl, message, requestId, orderId) = 
                await _momoService.CreatePaymentRequestAsync(contractUid, consumer);

            if (!success)
            {
                _logger.LogWarning("Failed to create MoMo payment: {Message}", message);
                return BadRequest(new
                {
                    success = false,
                    message = message,
                    contractUid = contractUid
                });
            }

            _logger.LogInformation("MoMo payment created successfully for contract {ContractUid}", contractUid);

            return Ok(new
            {
                success = true,
                message = "Payment request created successfully",
                data = new
                {
                    payUrl = payUrl,
                    requestId = requestId,
                    orderId = orderId,
                    contractUid = contractUid
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating MoMo payment for contract {ContractUid}", contractUid);
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while creating payment request"
            });
        }
    }

}
