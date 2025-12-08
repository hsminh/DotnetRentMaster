using Microsoft.AspNetCore.Mvc;
using RentMaster.partner.Firebase.Services.Interfaces;
using RentMaster.Core.Middleware;
using RentMaster.Accounts.LandLords.Models;

namespace RentMaster.Notification.Controllers
{
    [ApiController]
    [Attributes.LandLordScope]
    [Route("admin/api/notifications")]
    public class NotificationController : ControllerBase
    {
        private readonly IUserChannelNotificationService _notificationService;

        public NotificationController(IUserChannelNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var landlord = HttpContext.GetCurrentUser<LandLord>();
            var channel = $"notification-landlord-{landlord.Uid}";
            await _notificationService.MarkAllAsReadAsync(channel);
            return Ok();
        }
    }

}