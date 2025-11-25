namespace RentMaster.partner.Firebase.Models;

public class ChatMessage
{
    public string Content { get; set; } = string.Empty;
    public string Sender { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("O");
}
