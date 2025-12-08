using Microsoft.AspNetCore.Mvc;
using RentMaster.Accounts.Consumers.Types;
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
        [NonAction]
        public override async Task<IActionResult> Update(Guid id, [FromBody] Consumer model)
        {
            return await base.Update(id, model);
        }
    
        
        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(50_000_000)]
        public async Task<IActionResult> UpdateProfile(Guid id, [FromForm] ConsumerRequest request)
        {
            try
            {
                var updatedConsumer = await _consumerService.UpdateConsumerProfile(id, request);
                return Ok(new 
                {
                    updatedConsumer.Uid,
                    updatedConsumer.FirstName,
                    updatedConsumer.LastName,
                    updatedConsumer.Gmail,
                    updatedConsumer.PhoneNumber,
                    updatedConsumer.Avatar,
                    updatedConsumer.IsVerified,
                    updatedConsumer.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    status = "error",
                    message = "An error occurred while updating the profile",
                    error = ex.Message 
                });
            }
        }
    }
}