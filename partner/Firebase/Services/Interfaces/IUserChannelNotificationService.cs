namespace RentMaster.partner.Firebase.Services.Interfaces;

public interface IUserChannelNotificationService
{
    Task SendToUserChannelAsync(string channel, string message);
}
