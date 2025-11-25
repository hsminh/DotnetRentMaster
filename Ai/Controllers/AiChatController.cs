using Microsoft.AspNetCore.Mvc;
using RentMaster.Accounts.Models;
using RentMaster.Ai.Interfaces;
using RentMaster.partner.Firebase.Models;
using RentMaster.partner.Firebase.Services.Interfaces;
using RentMaster.Core.Middleware;

namespace RentMaster.Ai.Controllers;

[ApiController]
[Route("consumer/api/chat-bot")]
// [Attributes.UserScope] 
public class AiChatController : ControllerBase
{
    private readonly IGoogleAiService _aiService;
    private readonly IUserChannelNotificationService _firebaseService;
    private readonly ILogger<AiChatController> _logger;

    public AiChatController(
        IGoogleAiService aiService,
        IUserChannelNotificationService firebaseService,
        ILogger<AiChatController> logger)
    {
        _aiService = aiService;
        _firebaseService = firebaseService;
        _logger = logger;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> AskQuestion([FromBody] CreateAiQuestionRequest request)
    {
        try
        {
            // var user = HttpContext.GetCurrentUser<Consumer>();
            //
            // if (user == null)
            // {
            //     return Unauthorized(new { message = "User not found" });
            // }
            //
            // var consumerId = user.Uid.ToString();
            var channel = $"chatbot-ai-3a6560b3-6149-480d-a13b-d3b6d2de0f12-chatbot";
            // Send user message
            var userMessage = new ChatMessage
            {
                Content = request.Question,
                Sender = "user",
                SenderId = "3a6560b3-6149-480d-a13b-d3b6d2de0f12",
                Timestamp = DateTime.UtcNow.ToString("O")
            };
            
            await _firebaseService.SendToUserChannelAsync(channel, userMessage);
        
            var aiResponse = await _aiService.AskAsync(request.Question);
        
            var aiMessage = new ChatMessage
            {
                Content = aiResponse,
                Sender = "ai",
                SenderId = "system",
                Timestamp = DateTime.UtcNow.ToString("O")
            };
            
            await _firebaseService.SendToUserChannelAsync(channel, aiMessage);

            return Ok(new 
            { 
                message = "Question processed successfully",
                timestamp = DateTimeOffset.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AI question");
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }
}