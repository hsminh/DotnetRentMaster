using FirebaseAdmin.Messaging;
using RentMaster.partner.Firebase.Services.Client;

namespace RentMaster.partner.Firebase.Services;

public interface IUserChannelNotificationService
{
    Task SendToUserChannelAsync(string channel, string message);
}

public class FirebasePubSubService : IUserChannelNotificationService
{
    private readonly ILogger<FirebasePubSubService> _logger;
    private readonly FirebaseMessaging _messaging;

    public FirebasePubSubService(ILogger<FirebasePubSubService> logger)
    {
        _logger = logger;
        FirebaseClientConfig.Init(logger);
        _messaging = FirebaseMessaging.DefaultInstance;
    }

    public async Task SendToUserChannelAsync(string channel, string message)
    {
        if (string.IsNullOrWhiteSpace(channel))
            throw new ArgumentException("Channel cannot be empty", nameof(channel));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty", nameof(message));

        // Validate topic name format
        if (!IsValidTopicName(channel))
        {
            throw new ArgumentException($"Invalid topic name: {channel}. Topic names must contain only letters, numbers, and underscores.");
        }

        var msg = new Message
        {
            Topic = channel,
            Notification = new Notification
            {
                Title = "New Notification",
                Body = message
            },
            Data = new Dictionary<string, string>
            {
                { "channel", channel },
                { "message", message },
                { "timestamp", DateTime.UtcNow.ToString("O") }
            },
            Android = new AndroidConfig
            {
                Priority = Priority.High
            },
            Apns = new ApnsConfig
            {
                Headers = new Dictionary<string, string>
                {
                    { "apns-priority", "10" }
                }
            }
        };

        await SendWithRetryAsync(msg, channel);
    }

    private async Task SendWithRetryAsync(Message message, string channel, int maxRetries = 3)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30)); // Giảm timeout
                
                _logger.LogInformation($"Attempt {attempt} to send message to channel '{channel}'");
                
                var response = await _messaging.SendAsync(message, cts.Token);
                
                _logger.LogInformation($"Successfully sent message to channel '{channel}': {response}");
                return;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                _logger.LogWarning(ex, $"Attempt {attempt} failed for channel '{channel}'. Retrying in {attempt} seconds...");
                await Task.Delay(TimeSpan.FromSeconds(attempt * 1000)); // Exponential backoff
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"All {maxRetries} attempts failed for channel '{channel}'");
                throw new Exception($"Failed to send message after {maxRetries} attempts", ex);
            }
        }
    }

    private static bool IsValidTopicName(string topic)
    {
        if (string.IsNullOrWhiteSpace(topic)) return false;
        
        // Topic names must match regex: [a-zA-Z0-9-_.~%]+
        return System.Text.RegularExpressions.Regex.IsMatch(topic, @"^[a-zA-Z0-9-_.~%]+$");
    }
}