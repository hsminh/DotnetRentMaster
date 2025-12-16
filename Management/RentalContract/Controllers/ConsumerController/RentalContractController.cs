using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentMaster.Core.Middleware;
using RentMaster.Data;
using RentMaster.Management.RentalContract.Services;
using RentMaster.Management.RentalContract.Types.Response;

namespace RentMaster.Management.RentalContract.Controllers.ConsumerController;

[ApiController]
[Attributes.UserScope]
[Route("consumers/api/contracts")]
public class ConsumerRentalContractController : ControllerBase
{
    private readonly RentalContractService _service;
    private readonly AppDbContext _context;

    public ConsumerRentalContractController(RentalContractService service, AppDbContext context)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var consumer = HttpContext.GetCurrentUser<Accounts.Models.Consumer>();
        var contracts = await _service.GetContractsByParticipantAsync(consumer.Uid);
        var response = new List<RentalContractResponseDto>();
        foreach (var contract in contracts)
        {
            response.Add(await MapToResponseAsync(contract));
        }
        return Ok(response);
    }

    private async Task<RentalContractResponseDto> MapToResponseAsync(RentalContractResponseDto contract)
    {
        object? apartmentDetails = null;

        if (contract.Type.Equals("FullApartment", StringComparison.OrdinalIgnoreCase))
        {
            apartmentDetails = await _context.Apartments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Uid == contract.ApartmentUid);
        }
        else if (contract.Type.Equals("RoomBased", StringComparison.OrdinalIgnoreCase))
        {
            apartmentDetails = await _context.ApartmentRooms
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Uid == contract.ApartmentUid);
        }

        contract.ApartmentDetails = apartmentDetails;
        return contract;
    }
}

