using Microsoft.AspNetCore.Mvc;
using RentMaster.Accounts.Models;
using RentMaster.Core.Controllers;
using RentMaster.Core.Middleware;
using RentMaster.RealEstate.Models;
using RentMaster.RealEstate.Services;
using RentMaster.RealEstate.Types.Request;

namespace RentMaster.RealEstate.Controllers;

[ApiController]
[Attributes.LandLordScope]
[Route("landlords/api/[controller]")]
public class ApartmentController : BaseController<Apartment>
{
    private readonly ApartmentService _apartmentService;

    public ApartmentController(ApartmentService apartmentService)
        : base(apartmentService)
    {
        _apartmentService = apartmentService;
    }
    
    [HttpGet]
    public override async Task<IActionResult> GetAll()
    {
        var landlord = HttpContext.GetCurrentUser<LandLord>();
        var apartments = await _apartmentService.getApartments(landlord);
        return Ok(apartments);
    }
    
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Create([FromForm] ApartmentCreateRequest request)
    {
        var landlord = HttpContext.GetCurrentUser<LandLord>();
        var result = await _apartmentService.CreateApartmentAsync(landlord, request);

        return Ok(new
        {
            Message = "apartment_created_successfully",
            Data = result
        });
    }

    [ApiExplorerSettings(IgnoreApi = true)]
    public override async Task<IActionResult> Create([FromBody] Apartment model)
    {
        return await base.Create(model);
    }
    
    [HttpGet("{id}")]
    public override async Task<IActionResult> GetByUid(Guid id)
    {
        var landlord = HttpContext.GetCurrentUser<LandLord>();
        var apartment = await _apartmentService.GetApartment(landlord, id);

        if (apartment == null)
            return NotFound(new { message = "apartment_not_found" });

        return Ok(apartment);
    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(50_000_000)]
    public new async Task<IActionResult> Update(Guid id, [FromForm] ApartmentCreateRequest request)
    {
        if (request == null)
            return BadRequest("Invalid request.");

        var landlord = HttpContext.GetCurrentUser<LandLord>();
        var result = await _apartmentService.UpdateApartment(landlord, id, request);

        return Ok(new
        {
            Message = "apartment_updated_successfully",
            Data = result
        });
    }

} 