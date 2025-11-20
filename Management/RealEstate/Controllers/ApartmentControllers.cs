using Microsoft.AspNetCore.Mvc;
using RentMaster.Core.Controllers;
using RentMaster.Core.Middleware;
using RentMaster.Accounts.LandLords.Models;
using RentMaster.Management.RealEstate.Services;
using RentMaster.Management.RealEstate.Types.Request;
using RentMaster.Management.RealEstate.Models;
using FluentValidation;

namespace RentMaster.Management.RealEstate.Controllers;

[ApiController]
[Attributes.LandLordScope]
[Route("landlords/api/apartment")]
public class ApartmentController : BaseController<Apartment>
{
    private readonly ApartmentService _service;
    private readonly IValidator<ApartmentCreateRequest> _validator;

    public ApartmentController(
        ApartmentService service,
        IValidator<ApartmentCreateRequest> validator) : base(service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    [HttpGet]
    public override async Task<IActionResult> GetAll()
    {
        var landlord = HttpContext.GetCurrentUser<LandLord>();
        var apartments = await _service.getApartments(landlord);
        return Ok(apartments);
    }

    [HttpGet("{id}")]
    public override async Task<IActionResult> GetByUid(Guid id)
    {
        var landlord = HttpContext.GetCurrentUser<LandLord>();
        var apartment = await _service.GetApartment(landlord, id);

        if (apartment == null)
            return NotFound(new { message = "Apartment_not_found" });

        return Ok(apartment);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Create([FromForm] ApartmentCreateRequest request)
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
            var apartment = await _service.CreateApartmentAsync(landlord, request);

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Apartment created successfully",
                Data = apartment
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "An error occurred while creating the apartment",
                Error = ex.Message
            });
        }
    }
    [NonAction]
    public override async Task<IActionResult> Update(Guid id, [FromBody] Apartment model)
    {
        return await base.Update(id, model);
    }
    
    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Update(Guid id, [FromForm] ApartmentCreateRequest request)
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
            var apartment = await _service.UpdateApartment(landlord, id, request);

            if (apartment == null)
            {
                return NotFound(new
                {
                    StatusCode = StatusCodes.Status404NotFound,
                    Message = "Apartment not found"
                });
            }

            return Ok(new
            {
                StatusCode = StatusCodes.Status200OK,
                Message = "Apartment updated successfully",
                Data = apartment
            });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "An error occurred while updating the apartment",
                Error = ex.Message
            });
        }
    }
}
