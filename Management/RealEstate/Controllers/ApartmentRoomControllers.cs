using Microsoft.AspNetCore.Mvc;
using RentMaster.Core.Controllers;
using RentMaster.Core.Middleware;
using RentMaster.RealEstate.Models;
using FluentValidation;
using RentMaster.Accounts.LandLords.Models;
using RentMaster.Management.RealEstate.Services;
using RentMaster.Management.RealEstate.Types.Request;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System.Linq;

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
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
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
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Validation failed",
                Errors = validationResult.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Error = e.ErrorMessage
                })
            });
        }

        try
        {
            var landlord = HttpContext.GetCurrentUser<LandLord>();
            var room = await _service.CreateApartmentRoomAsync(landlord, request);
            
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Apartment room created successfully",
                Data = room
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "An error occurred while creating the apartment room",
                Error = ex.Message
            });
        }
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
                StatusCode = StatusCodes.Status400BadRequest,
                Message = "Validation failed",
                Errors = validationResult.Errors.Select(e => new
                {
                    Field = e.PropertyName,
                    Error = e.ErrorMessage
                })
            });
        }

        try
        {
            var landlord = HttpContext.GetCurrentUser<LandLord>();
            var room = await _service.UpdateApartmentRoomAsync(landlord, id, request);

            if (room == null) 
            {
                return NotFound(new 
                { 
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Apartment room not found" 
                });
            }
            
            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Apartment room updated successfully",
                Data = room
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "An error occurred while updating the apartment room",
                Error = ex.Message
            });
        }
    }
    [NonAction]
    public override async Task<IActionResult> Update(Guid id, [FromBody] ApartmentRoom model)
    {
        return await base.Update(id, model);
    }

}