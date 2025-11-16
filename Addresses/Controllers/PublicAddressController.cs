using Microsoft.AspNetCore.Mvc;
using RentMaster.Addresses.Models;
using RentMaster.Addresses.Services;

namespace RentMaster.Addresses.Controllers
{
    [ApiController]
    [Route("public/address")]
    public class PublicAddressController : ControllerBase
    {
        private readonly AddressService _service;
        private readonly ILogger<PublicAddressController> _logger;

        public PublicAddressController(AddressService service, ILogger<PublicAddressController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("province")]
        public async Task<IActionResult> GetProvinces()
        {
            try
            {
                var provinces = await _service.GetAddressAsync(DivisionType.Province);
                return Ok(provinces);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting provinces");
                return StatusCode(500, "An error occurred while getting provinces");
            }
        }

        [HttpGet("ward")]
        public async Task<IActionResult> GetWards([FromQuery] string? parentCode)
        {
            try
            {
                var wards = await _service.GetAddressAsync(DivisionType.Ward, parentCode);
                return Ok(wards);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wards for parent {ParentCode}", parentCode);
                return StatusCode(500, "An error occurred while getting wards");
            }
        }
    }
}