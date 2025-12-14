    using Microsoft.AspNetCore.Mvc;
    using RentMaster.Addresses.Services;
    using RentMaster.Core.Middleware;

    namespace RentMaster.Addresses.Controllers
    {
        [ApiController]
        [Attributes.AdminScope]
        [Route("admin/api/address")]
        public class AddressImportController : ControllerBase
        {
            private readonly IAddressImportService _addressImportService;
            private readonly ILogger<AddressImportController> _logger;

            public AddressImportController(
                IAddressImportService addressImportService,
                ILogger<AddressImportController> logger)
            {
                _addressImportService = addressImportService;
                _logger = logger;
            }

            [HttpPost("import")]
            public async Task<IActionResult> Import([FromForm] IFormFile file)    
            {
                if (file == null)
                    return BadRequest(new { success = false, message = "File is required" });

                var result = await _addressImportService.ImportFromCsvAsync(file);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
        }
    }