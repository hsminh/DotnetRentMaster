using Microsoft.AspNetCore.Mvc;
using IUserChannelNotificationService = RentMaster.partner.Firebase.Services.Interfaces.IUserChannelNotificationService;

[ApiController]
[Route("api/test")]
public class FirebaseTestController : ControllerBase
{
    private readonly IUserChannelNotificationService _firebaseService;

    public FirebaseTestController(IUserChannelNotificationService firebaseService)
    {
        _firebaseService = firebaseService;
    }

    [HttpPost("firebase")]
    public async Task<IActionResult> TestFirebase()
    {
        await _firebaseService.SendToUserChannelAsync("minhdz", "Hello from API via Realtime DB");
        return Ok(new { status = "ok" });
    }
}