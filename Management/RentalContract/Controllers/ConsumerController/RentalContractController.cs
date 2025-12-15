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
        var response = await Task.WhenAll(contracts.Select(c => MapToResponseAsync(c)));
        return Ok(response);
    }

    private async Task<RentalContractResponseDto> MapToResponseAsync(Models.RentalContract contract)
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

        return new()
        {
            Uid = contract.Uid,
            LandlordUid = contract.LandlordUid,
            ApartmentUid = contract.ApartmentUid,
            Type = contract.Type,
            ResponsibleUid = contract.ResponsibleUid,
            ParticipantUids = contract.ParticipantUids,
            MonthlyPrice = contract.MonthlyPrice,
            DepositAmount = contract.DepositAmount,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            Status = contract.Status.ToString(),
            CreatedAt = contract.CreatedAt,
            ApartmentDetails = apartmentDetails
        };
    }
}

