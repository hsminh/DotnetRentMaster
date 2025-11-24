using Microsoft.AspNetCore.Mvc;
using RentMaster.partner.Firebase.Services;

namespace RentMaster.partner.Firebase.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FirebaseTestController : ControllerBase
{
    private readonly IUserChannelNotificationService _userChannelNotificationService;

    public FirebaseTestController(IUserChannelNotificationService userChannelNotificationService)
    {
        _userChannelNotificationService = userChannelNotificationService;
    }

    public class SendMessageRequest
    {
        public string Channel { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendMessageRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Channel) || string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "Channel and Message are required" });
        }

        try
        {
            await _userChannelNotificationService.SendToUserChannelAsync(
                request.Channel,
                request.Message
            );

            return Ok(new
            {
                message = "Notification sent successfully",
                channel = request.Channel
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Error sending notification",
                details = ex.Message
            });
        }
    }
}