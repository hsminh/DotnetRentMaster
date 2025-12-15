using Microsoft.AspNetCore.Mvc;
using RentMaster.Accounts.LandLords.Models;
using RentMaster.Core.Backend.Auth.Types.enums;
using RentMaster.Core.Controllers;
using RentMaster.Core.Middleware;
using RentMaster.Management.ConsumerContact.enums;
using RentMaster.Management.ConsumerContact.Models;
using RentMaster.Management.RealEstate.Models;
using RentMaster.Management.ConsumerContact.Request;
using RentMaster.Management.ConsumerContact.Services;
using RentMaster.Management.RealEstate.Models; // For Apartment model

namespace RentMaster.Management.ConsumerContact.Controllers.LandlordController;

[ApiController]
[Attributes.LandLordScope]
[Route("landlords/api/tenant")]
public class ConsumerContactController : BaseController<Models.ConsumerContact>
{
    private readonly ConsumerContactService _service;

    public ConsumerContactController(
        ConsumerContactService service) : base(service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }
    
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetContactDetails(Guid id)
    {
        try
        {
            var landlord = HttpContext.GetCurrentUser<LandLord>();
            var contact = await _service.GetConsumerContactDetails(id, landlord.Uid);
        
            if (contact == null)
                return NotFound(new { message = "Contact not found" });

            var response = new ConsumerContactResponseDto()
            {
                Uid = contact.Uid,
                Status = contact.Status.ToString(),
                Type = contact.Type,
                CreatedAt = contact.CreatedAt,
                Consumer = contact.Consumer,
                RealEstateUnit = contact.Apartment != null
                    ? (object)contact.Apartment 
                    : (contact.ApartmentRoom != null
                        ? (object)contact.ApartmentRoom
                        : null)
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while fetching contact details" });
        }
    }
    
    [HttpGet("filtered")]
    public async Task<IActionResult> GetForLandlord(
        [FromQuery] string? type,
        [FromQuery] string? status
    )
    {
        var landlord = HttpContext.GetCurrentUser<LandLord>();

        var contacts = await _service
            .GetConsumerContactsFiltered(
                landlord.Uid,
                UserTypes.LandLord,
                type,
                status
            );

        return Ok(contacts);
    }
    
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var contact = await _service.GetByUidAsync(id);
        if (contact == null)
            return NotFound();

        if (Enum.TryParse<JoinApartmentStatus>(request.Status, true, out var status))
        {
            contact.Status = status;
            await _service.UpdateAsync(contact);
            return NoContent();
        }

        return BadRequest(new { message = "Invalid status value. Must be 'Pending', 'Approved', or 'Rejected'." });
    }

    public class UpdateStatusRequest
    {
        public string Status { get; set; }  
    }

}
