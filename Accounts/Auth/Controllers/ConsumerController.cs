using Microsoft.AspNetCore.Mvc;
using RentMaster.Accounts.Models;
using RentMaster.Accounts.Services;
using RentMaster.Core.Controllers;

namespace RentMaster.Controllers
{
    [ApiController]
    [Route("[controller]/api")]
    public class ConsumerController : BaseController<Consumer>
    {
        private readonly ConsumerService _consumerService;

        public ConsumerController(ConsumerService service)
            : base(service)
        {
            _consumerService = service;
        }

        [HttpPost("{uid:guid}/check-verified")]
        public async Task<IActionResult> CheckVerified(Guid uid)
        {
            try
            {
                var consumer = await Service.GetByUidAsync(uid);
                if (consumer == null)
                    return NotFound(new { message = "Consumer not found" });
        
                var isVerified = await _consumerService.CheckAndVerifyAsync(consumer);
                
                return Ok(new 
                { 
                    is_verified = isVerified,
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}