using Microsoft.AspNetCore.Mvc;
using RentMaster.Accounts.LandLords.Models;
using RentMaster.Core.Middleware;
using RentMaster.Management.RentalContract.Services;
using RentMaster.Management.RentalContract.Types.Request;
using RentMaster.Management.RentalContract.Types.Response;

namespace RentMaster.Management.RentalContract.Controllers;

[ApiController]
[Attributes.LandLordScope]
[Route("landlords/api/payments")]
public class RentalContractMonthlyPaymentController : ControllerBase
{
    private readonly RentalContractMonthlyPaymentService _service;

    public RentalContractMonthlyPaymentController(RentalContractMonthlyPaymentService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RentalContractMonthlyPaymentCreateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var payment = await _service.CreatePaymentAsync(request);
            return CreatedAtAction(nameof(GetByUid), new { id = payment.Uid }, MapToResponse(payment));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByUid(Guid id)
    {
        var payment = await _service.GetPaymentAsync(id);
        if (payment == null)
            return NotFound(new { message = "Payment not found" });

        return Ok(MapToResponse(payment));
    }

    [HttpGet("contract/{contractId:guid}")]
    public async Task<IActionResult> GetByContract(Guid contractId)
    {
        var payments = await _service.GetPaymentsByContractAsync(contractId);
        return Ok(payments.Select(MapToResponse));
    }

    [HttpGet("contract/{contractId:guid}/unpaid")]
    public async Task<IActionResult> GetUnpaidByContract(Guid contractId)
    {
        var payments = await _service.GetUnpaidPaymentsAsync(contractId);
        return Ok(payments.Select(MapToResponse));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id,
        [FromBody] RentalContractMonthlyPaymentUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var payment = await _service.UpdatePaymentAsync(id, request);
        if (payment == null)
            return NotFound(new { message = "Payment not found" });

        return Ok(MapToResponse(payment));
    }

    [HttpPut("{id:guid}/mark-paid")]
    public async Task<IActionResult> MarkAsPaid(Guid id, [FromQuery] string? method = null)
    {
        var landlord = HttpContext.GetCurrentUser<LandLord>();
        var payment = await _service.MarkAsPaidAsync(id, landlord.Uid, method);
        if (payment == null)
            return NotFound(new { message = "Payment not found" });

        return Ok(MapToResponse(payment));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeletePaymentAsync(id);
        return deleted
            ? NoContent()
            : NotFound(new { message = "Payment not found" });
    }

    private RentalContractMonthlyPaymentResponseDto MapToResponse(Models.RentalContractMonthlyPayment payment)
        => new()
        {
            Uid = payment.Uid,
            RentalContractUid = payment.RentalContractUid,
            Year = payment.Year,
            Month = payment.Month,
            Amount = payment.Amount,
            IsPaid = payment.IsPaid,
            PaidAt = payment.PaidAt,
            CollectedByUid = payment.CollectedByUid,
            Note = payment.Note,
            Method = payment.Method,
            CreatedAt = payment.CreatedAt
        };
}
