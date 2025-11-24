namespace RentMaster.partner.Firebase.Services;
using FirebaseAdmin.Messaging;

public interface IFirebasePushService
{
    Task SendToDeviceAsync(string deviceToken, string title, string body, Dictionary<string, string>? data = null);
}

public class FirebasePushService : IFirebasePushService
{
    public async Task SendToDeviceAsync(string deviceToken, string title, string body, Dictionary<string, string>? data = null)
    {
        var message = new Message
        {
            Token = deviceToken,
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Data = data ?? new Dictionary<string, string>()
        };

        var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        Console.WriteLine($"FCM sent: {response}");
    }
}