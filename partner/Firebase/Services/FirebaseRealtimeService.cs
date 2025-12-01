using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RentMaster.partner.Firebase.Models;
using RentMaster.partner.Firebase.Services.Interfaces;

namespace RentMaster.partner.Firebase.Services
{
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

        public async Task SendToChannelAsync(string channel, ChatMessage message)
        {
            if (string.IsNullOrWhiteSpace(channel))
                throw new ArgumentException("Channel cannot be empty", nameof(channel));

            if (message == null)
                throw new ArgumentNullException(nameof(message));

            var response = await _httpClient.PostAsJsonAsync($"{channel}.json", message);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to write to Realtime DB. Status: {Status}, Body: {Body}", response.StatusCode, body);
                throw new Exception($"Realtime DB write failed: {response.StatusCode}");
            }

            _logger.LogInformation("Message sent to channel '{Channel}'", channel);
        }

        public async Task<IEnumerable<ChatMessage>> GetChannelAsync(string channel)
        {
            if (string.IsNullOrWhiteSpace(channel))
                throw new ArgumentException("Channel cannot be empty", nameof(channel));

            try
            {
                var response = await _httpClient.GetStringAsync($"{channel}.json");
                if (string.IsNullOrWhiteSpace(response) || response == "null")
                    return Enumerable.Empty<ChatMessage>();

                var messagesDict = JsonSerializer.Deserialize<Dictionary<string, ChatMessage>>(response);
                if (messagesDict != null)
                {
                    return messagesDict.Values;
                }

                var messagesList = JsonSerializer.Deserialize<List<ChatMessage>>(response);
                if (messagesList != null)
                {
                    return messagesList;
                }

                return Enumerable.Empty<ChatMessage>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse Firebase channel '{Channel}'", channel);
                return Enumerable.Empty<ChatMessage>();
            }
        }

        public async Task UpdateChannelAsync(string channel, IEnumerable<ChatMessage> messages)
        {
            if (messages == null) return;
            await _httpClient.PutAsJsonAsync($"{channel}.json", messages);
            _logger.LogInformation("Channel '{Channel}' updated with {Count} messages", channel, messages.Count());
        }

        public async Task MarkAllAsReadAsync(string channel)
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"{channel}.json");
                if (string.IsNullOrWhiteSpace(response) || response == "null")
                {
                    _logger.LogInformation("No messages found in channel '{Channel}'", channel);
                    return;
                }

                var messagesDict = JsonSerializer.Deserialize<Dictionary<string, ChatMessage>>(response);
                if (messagesDict == null || !messagesDict.Any())
                {
                    _logger.LogInformation("No messages found in channel '{Channel}'", channel);
                    return;
                }

                foreach (var kvp in messagesDict)
                {
                    if (kvp.Value.IsRead != "True")
                    {
                        kvp.Value.IsRead = "True";
                        await _httpClient.PatchAsJsonAsync($"{channel}/{kvp.Key}.json", 
                            new { IsRead = "True" });
                    }
                }
                _logger.LogInformation("All messages marked as read in channel '{Channel}'", channel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking messages as read in channel '{Channel}'", channel);
                throw;
            }
        }
    }
}
