using Microsoft.AspNetCore.Mvc;
using RentMaster.Core.Controllers;
using RentMaster.Core.Exceptions;
using RentMaster.Core.Middleware;
using RentMaster.Management.ConsumerContact.Services;
using RentMaster.partner.Firebase.Services.Interfaces;
using RentMaster.Management.ConsumerContact.Request;

namespace RentMaster.Management.ConsumerContact.Controllers
{
    [ApiController]
    [Route("consumer/api/contact")]
    [Attributes.UserScope]
    public class ConsumerContactController : BaseController<ConsumerContact.Models.ConsumerContact>
    {
        private readonly ConsumerContactService _consumerContactService;
        private readonly IUserChannelNotificationService _firebaseService;
        private readonly ILogger<ConsumerContactController> _logger;
        private readonly IConfiguration _configuration;

        public ConsumerContactController(
            ConsumerContactService service,
            IUserChannelNotificationService firebaseService,
            ILogger<ConsumerContactController> logger,
            IConfiguration configuration)   
            : base(service)
        {
            _consumerContactService = service;
            _firebaseService = firebaseService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("join")]
        public async Task<IActionResult> JoinApartment([FromBody] JoinApartmentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var user = HttpContext.GetCurrentUser<Accounts.Models.Consumer>();
                
                if (user == null)
                {
                    return Unauthorized(new { message = "User not found" });
                }
                
                var contact = await _consumerContactService.AddConsumerToApartment(
                    user.Uid,
                    request.LandlordUid,
                    request.ApartmentUid
                );

                // Prepare notification data
                var notificationData = new Dictionary<string, object>
                {
                    { "apartmentId", request.ApartmentUid },
                    { "consumerId", user.Uid },
                    { "timestamp", DateTime.UtcNow },
                    { "type", "JOIN_REQUEST" },
                    { "title", "New Join Request" },
                    { "body", $"{user.FirstName} {user.LastName} has requested to join your apartment." }
                };
                
                var message = new ChatMessage
                {
                    Content = $"{user.FirstName} {user.LastName} has requested to join your apartment.",
                    Sender = "system",
                    SenderId = user.Uid.ToString(),
                    Link = $"http://localhost:3000/landlord/tenants/{contact.Uid}/edit",
                    Type = "JOIN_REQUEST",
                    IsRead = "False",
                    Data = notificationData,
                    Timestamp = DateTime.UtcNow.ToString("O")
                };

                var channel = $"notification-landlord-{request.LandlordUid}";
                try
                {
                    await _firebaseService.SendToChannelAsync(channel, message);
                    _logger.LogInformation("Notification sent to landlord {LandlordId}", request.LandlordUid);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send notification to landlord {LandlordId}", request.LandlordUid);
                }

                return Ok(new { 
                    message = "Join request submitted successfully",
                    data = contact
                });
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing join apartment request");
                return StatusCode(500, new { message = "An error occurred while processing your request." });
            }
        }
    }
}