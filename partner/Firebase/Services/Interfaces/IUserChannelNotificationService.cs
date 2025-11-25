using RentMaster.partner.Firebase.Models;

namespace RentMaster.partner.Firebase.Services.Interfaces;

public interface IUserChannelNotificationService
{
    Task SendToUserChannelAsync(string channel, ChatMessage message);
    
    // Keep the old method for backward compatibility
    Task SendToUserChannelAsync(string channel, string message);
}
