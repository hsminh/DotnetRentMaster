using RentMaster.partner.Firebase.Models;

namespace RentMaster.partner.Firebase.Services.Interfaces;

public interface IUserChannelNotificationService
{
    Task SendToChannelAsync(string channel, ChatMessage message);

    Task<IEnumerable<ChatMessage>> GetChannelAsync(string channel);

    Task UpdateChannelAsync(string channel, IEnumerable<ChatMessage> messages);

    Task MarkAllAsReadAsync(string channel);
    
}
