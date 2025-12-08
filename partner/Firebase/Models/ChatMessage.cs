using System.Text.Json.Serialization;

public class ChatMessage
{
    public string Content { get; set; }
    public string Sender { get; set; }
    public string SenderId { get; set; }
    public string Link { get; set; }
    public string Type { get; set; }
    public string IsRead { get; set; }
    public Dictionary<string, object> Data { get; set; }
    public string Timestamp { get; set; }

    [JsonIgnore]
    public bool IsReadBool => IsRead?.ToLower() == "true";
}