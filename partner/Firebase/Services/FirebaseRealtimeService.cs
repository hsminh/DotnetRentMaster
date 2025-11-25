using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using RentMaster.partner.Firebase.Models;
using RentMaster.partner.Firebase.Services.Interfaces;

namespace RentMaster.partner.Firebase.Services;

public class FirebaseRealtimeService : IUserChannelNotificationService
{
    private readonly ILogger<FirebaseRealtimeService> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _databaseUrl;

    public FirebaseRealtimeService(ILogger<FirebaseRealtimeService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;

        _databaseUrl = Environment.GetEnvironmentVariable("FIREBASE_DATABASE_URL") 
                       ?? "https://rentmaster-4d3e5-default-rtdb.asia-southeast1.firebasedatabase.app";

        _httpClient.BaseAddress = new Uri(_databaseUrl);
    }

    public async Task SendToUserChannelAsync(string channel, ChatMessage message)
    {
        if (string.IsNullOrWhiteSpace(channel)) 
            throw new ArgumentException("Channel cannot be empty", nameof(channel));
        if (message == null) 
            throw new ArgumentNullException(nameof(message));

        try
        {
            _logger.LogInformation("Posting to Realtime DB: {Url}", channel);

            var response = await _httpClient.PostAsJsonAsync($"{channel}.json", message);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to write to Realtime DB. Status: {Status}, Body: {Body}", response.StatusCode, body);
                throw new Exception($"Realtime DB write failed: {response.StatusCode}");
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Successfully wrote to Realtime DB for channel '{Channel}'. Response: {Response}", channel, responseBody);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Realtime DB for channel '{Channel}'", channel);
            throw;
        }
    }

    public async Task SendToUserChannelAsync(string channel, string message)
    {
        var chatMessage = new ChatMessage
        {
            Content = message,
            Sender = "system",
            SenderId = "system",
            Timestamp = DateTime.UtcNow.ToString("O")
        };

        await SendToUserChannelAsync(channel, chatMessage);
    }
}
