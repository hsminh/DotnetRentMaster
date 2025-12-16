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
    private readonly RentalContractMonthlyPaymentService _paymentService;
    private readonly ILogger<RentalContractMonthlyPaymentPaymentController> _logger;

    public RentalContractMonthlyPaymentPaymentController(
        RentalContractMonthlyPaymentMoMoService momoService,
        RentalContractMonthlyPaymentService paymentService,
        ILogger<RentalContractMonthlyPaymentPaymentController> logger)
    {
        _momoService = momoService;
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaymentsList()
    {
        try
        {
            var consumer = HttpContext.GetCurrentUser<Accounts.Models.Consumer>();
            _logger.LogInformation("Fetching payments list for consumer {ConsumerId}", consumer.Uid);

            var payments = await _paymentService.GetPaymentsByConsumerAsync(consumer.Uid);

            var paymentDtos = payments.Select(p => new Types.Response.RentalContractMonthlyPaymentResponseDto
            {
                Uid = p.Uid,
                RentalContractUid = p.RentalContractUid,
                Year = p.Year,
                Month = p.Month,
                Amount = p.Amount,
                IsPaid = p.IsPaid,
                PaidAt = p.PaidAt,
                CollectedByUid = p.CollectedByUid,
                Note = p.Note,
                Method = p.Method,
                CreatedAt = p.CreatedAt
            }).OrderByDescending(p => p.Year).ThenByDescending(p => p.Month);

            return Ok(new
            {
                success = true,
                message = "Payments retrieved successfully",
                data = paymentDtos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching payments list for consumer");
            return StatusCode(500, new
            {
                success = false,
                message = "An error occurred while fetching payments"
            });
        }
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
