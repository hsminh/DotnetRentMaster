using Microsoft.AspNetCore.Mvc;
using RentMaster.Accounts.LandLords.Models;
using RentMaster.Core.Controllers;
using RentMaster.Core.Middleware;
using RentMaster.Management.RentalContract.Services;
using RentMaster.Management.RentalContract.Types.Request;
using RentMaster.Management.RentalContract.Types.Response;

namespace RentMaster.Management.RentalContract.Controllers;

[ApiController]
[Attributes.LandLordScope]
[Route("landlords/api/contract")]
public class RentalContractController : BaseController<Models.RentalContract>
{
    private readonly RentalContractService _service;

    public RentalContractController(RentalContractService service) : base(service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    [HttpPost]
    public async Task<IActionResult> CreateContract([FromBody] RentalContractCreateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var contract = await _service.CreateContractAsync(request);
            var response = new RentalContractResponseDto
            {
                Uid = contract.Uid,
                ConsumerUid = contract.ConsumerUid,
                LandlordUid = contract.LandlordUid,
                ApartmentUid = contract.ApartmentUid,
                ApartmentRoomUid = contract.ApartmentRoomUid,
                MonthlyPrice = contract.MonthlyPrice,
                DepositAmount = contract.DepositAmount,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Status = contract.Status.ToString(),
                CreatedAt = contract.CreatedAt
            };

            return CreatedAtAction(nameof(GetContract), new { id = contract.Uid }, response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the contract" });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetContract(Guid id)
    {
        try
        {
            var contract = await _service.GetContractAsync(id);
            if (contract == null)
                return NotFound(new { message = "Contract not found" });

            var response = new RentalContractResponseDto
            {
                Uid = contract.Uid,
                ConsumerUid = contract.ConsumerUid,
                LandlordUid = contract.LandlordUid,
                ApartmentUid = contract.ApartmentUid,
                ApartmentRoomUid = contract.ApartmentRoomUid,
                MonthlyPrice = contract.MonthlyPrice,
                DepositAmount = contract.DepositAmount,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Status = contract.Status.ToString(),
                CreatedAt = contract.CreatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching the contract" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetContractsByLandlord()
    {
        try
        {
            var landlord = HttpContext.GetCurrentUser<LandLord>();
            var contracts = await _service.GetContractsByLandlordAsync(landlord.Uid);
            var response = contracts.Select(c => new RentalContractResponseDto
            {
                Uid = c.Uid,
                ConsumerUid = c.ConsumerUid,
                LandlordUid = c.LandlordUid,
                ApartmentUid = c.ApartmentUid,
                ApartmentRoomUid = c.ApartmentRoomUid,
                MonthlyPrice = c.MonthlyPrice,
                DepositAmount = c.DepositAmount,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = c.Status.ToString(),
                CreatedAt = c.CreatedAt
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching contracts" });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateContract(Guid id, [FromBody] RentalContractUpdateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var contract = await _service.UpdateContractAsync(id, request);
            if (contract == null)
                return NotFound(new { message = "Contract not found" });

            var response = new RentalContractResponseDto
            {
                Uid = contract.Uid,
                ConsumerUid = contract.ConsumerUid,
                LandlordUid = contract.LandlordUid,
                ApartmentUid = contract.ApartmentUid,
                ApartmentRoomUid = contract.ApartmentRoomUid,
                MonthlyPrice = contract.MonthlyPrice,
                DepositAmount = contract.DepositAmount,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Status = contract.Status.ToString(),
                CreatedAt = contract.CreatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while updating the contract" });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteContract(Guid id)
    {
        try
        {
            var result = await _service.DeleteContractAsync(id);
            if (!result)
                return NotFound(new { message = "Contract not found" });

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while deleting the contract" });
        }
    }

    [HttpGet("apartment/{apartmentId:guid}")]
    public async Task<IActionResult> GetContractsByApartment(Guid apartmentId)
    {
        try
        {
            var contracts = await _service.GetContractsByApartmentAsync(apartmentId);
            var response = contracts.Select(c => new RentalContractResponseDto
            {
                Uid = c.Uid,
                ConsumerUid = c.ConsumerUid,
                LandlordUid = c.LandlordUid,
                ApartmentUid = c.ApartmentUid,
                ApartmentRoomUid = c.ApartmentRoomUid,
                MonthlyPrice = c.MonthlyPrice,
                DepositAmount = c.DepositAmount,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = c.Status.ToString(),
                CreatedAt = c.CreatedAt
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching contracts" });
        }
    }
}
