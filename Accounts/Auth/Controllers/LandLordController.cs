using Microsoft.AspNetCore.Mvc;
using RentMaster.Accounts.Models;
using RentMaster.Accounts.Services;
using RentMaster.Core.Controllers;
using RentMaster.Core.Exceptions;

namespace RentMaster.Controllers
{
    [ApiController]
    [Route("landlord/api")]
    public class LandLordController : BaseController<LandLord>
    {
        private readonly LandLordService _landlordService;

        public LandLordController(LandLordService service)
            : base(service)
        {
            _landlordService = service;
        }

        [HttpPost("register")]
        public new async Task<IActionResult> Create([FromBody] LandLord request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdLandlord = await _landlordService.CreateAsync(request);

                return CreatedAtAction(nameof(GetByUid),
                    new { id = createdLandlord.Uid },
                    new
                    {
                        message = "landlord_registered_successfully",
                        data = createdLandlord
                    });
            }
            catch (ValidationException ex)
            {
                // Trả thẳng key-value
                return BadRequest(ex.Errors);
            }

            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}