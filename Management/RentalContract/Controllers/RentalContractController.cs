using Microsoft.AspNetCore.Mvc;
using RentMaster.Accounts.LandLords.Models;
using RentMaster.Core.Middleware;
using RentMaster.Management.RentalContract.Services;
using RentMaster.Management.RentalContract.Types.Request;
using RentMaster.Management.RentalContract.Types.Response;

namespace RentMaster.Management.RentalContract.Controllers;

[ApiController]
[Attributes.LandLordScope]
[Route("landlords/api/contracts")]
public class RentalContractController : ControllerBase
{
    private readonly RentalContractService _service;

    public RentalContractController(RentalContractService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] RentalContractCreateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var landlord = HttpContext.GetCurrentUser<LandLord>();
        request.LandlordUid = landlord.Uid;
        var contract = await _service.CreateContractAsync(request);
        return CreatedAtAction(
            nameof(GetByUid),
            new { id = contract.Uid },
            MapToResponse(contract)
        );
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetByUid(Guid id)
    {
        var contract = await _service.GetContractAsync(id);
        if (contract == null)
            return NotFound(new { message = "Contract not found" });

        return Ok(MapToResponse(contract));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var landlord = HttpContext.GetCurrentUser<LandLord>();
        var contracts = await _service.GetContractsByLandlordAsync(landlord.Uid);
        return Ok(contracts.Select(MapToResponse));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] RentalContractUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var contract = await _service.UpdateContractAsync(id, request);
        if (contract == null)
            return NotFound(new { message = "Contract not found" });

        return Ok(MapToResponse(contract));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteContractAsync(id);
        return deleted
            ? NoContent()
            : NotFound(new { message = "Contract not found" });
    }

    [HttpGet("apartment/{apartmentId:guid}")]
    public async Task<IActionResult> GetByApartment(Guid apartmentId)
    {
        var contracts = await _service.GetContractsByApartmentAsync(apartmentId);
        return Ok(contracts.Select(MapToResponse));
    }

    private RentalContractResponseDto MapToResponse(Models.RentalContract contract)
        => new()
        {
            Uid = contract.Uid,
            ConsumerUid = contract.ConsumerUid,
            LandlordUid = contract.LandlordUid,
            ApartmentUid = contract.ApartmentUid,
            Type = contract.Type,
            ResponsibleUid = contract.ResponsibleUid,
            ParticipantUidsJson = contract.ParticipantUidsJson,
            MonthlyPrice = contract.MonthlyPrice,
            DepositAmount = contract.DepositAmount,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            Status = contract.Status.ToString(),
            CreatedAt = contract.CreatedAt
        };
}

