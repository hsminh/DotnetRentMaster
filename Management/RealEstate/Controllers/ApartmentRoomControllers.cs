using Microsoft.AspNetCore.Mvc;
using RentMaster.Core.Controllers;
using RentMaster.Core.Middleware;
using RentMaster.RealEstate.Models;
using RentMaster.RealEstate.Types.Request;
using FluentValidation;
using RentMaster.Accounts.LandLords.Models;
using RentMaster.Management.RealEstate.Services;

namespace RentMaster.Management.RealEstate.Controllers;

[ApiController]
[Attributes.LandLordScope]
[Route("landlords/api/apartment-room")]
public class ApartmentRoomController : BaseController<ApartmentRoom>
{
    private readonly ApartmentRoomService _service;
    private readonly IValidator<ApartmentRoomCreateRequest> _validator;

    public ApartmentRoomController(
        ApartmentRoomService service,
        IValidator<ApartmentRoomCreateRequest> validator) : base(service)
    {
        _service = service;
        _validator = validator;
    }

    [HttpGet]
    public override async Task<IActionResult> GetAll()
    {
        var landlord = HttpContext.GetCurrentUser<LandLord>();
        var rooms = await _service.GetApartmentRooms(landlord);
        return Ok(rooms);
    }

    [HttpGet("{id}")]
    public override async Task<IActionResult> GetByUid(Guid id)
    {
        var landlord = HttpContext.GetCurrentUser<LandLord>();
        var room = await _service.GetApartmentRoom(landlord, id);
        if (room == null) return NotFound(new { message = "ApartmentRoom_not_found" });
        return Ok(room);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Create([FromForm] ApartmentRoomCreateRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                Message = "Validation_failed",
                Errors = validationResult.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Error = e.ErrorMessage
                })
            });
        }

        var landlord = HttpContext.GetCurrentUser<LandLord>();
        var room = await _service.CreateApartmentRoomAsync(landlord, request);
        return Ok(new
        {
            Message = "ApartmentRoom_created_successfully",
            Data = room
        });
    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Update(Guid id, [FromForm] ApartmentRoomCreateRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                Message = "Validation_failed",
                Errors = validationResult.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Error = e.ErrorMessage
                })
            });
        }

        var landlord = HttpContext.GetCurrentUser<LandLord>();
        var room = await _service.UpdateApartmentRoomAsync(landlord, id, request);

        if (room == null) return NotFound(new { message = "ApartmentRoom_not_found" });
        return Ok(new
        {
            Message = "ApartmentRoom_updated_successfully",
            Data = room
        });
    }
    [NonAction]
    public override async Task<IActionResult> Update(Guid id, [FromBody] ApartmentRoom model)
    {
        return await base.Update(id, model);
    }

}