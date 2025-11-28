using Microsoft.AspNetCore.Mvc;
using RentMaster.Addresses.Services;
namespace RentMaster.Addresses.Controllers;

[ApiController]
[Route("public/address")]
public class PublicAddressController : ControllerBase
{
    private readonly AddressService _service;

    public PublicAddressController(AddressService service, ILogger<PublicAddressController> logger)
    {
        _service = service;
    }

    [HttpGet("province")]
    public async Task<IActionResult> GetProvinces()
    {
        var provinces = await _service.GetProvincesAsync();
        return Ok(provinces);
    }

    [HttpGet("division")]
    public async Task<IActionResult> GetDivisions([FromQuery] string? parentUid)
    {
        if (string.IsNullOrWhiteSpace(parentUid))
            return BadRequest("parentUid is required");

        var divisions = await _service.GetChildrenByParentUidAsync(parentUid);
        return Ok(divisions);
    }
}