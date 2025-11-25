using Microsoft.AspNetCore.Mvc;
using RentMaster.Accounts.Models;
using RentMaster.Ai.Interfaces;
using RentMaster.partner.Firebase.Services.Interfaces;
using RentMaster.Core.Middleware;

namespace RentMaster.Ai.Controllers;

[ApiController]
[Route("consumer/api/chat-bot")]
[Attributes.UserScope] 
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
            var user = HttpContext.GetCurrentUser<Consumer>();
        
            if (user == null)
            {
                return Unauthorized(new { message = "User not found" });
            }

            var consumerId = user.Uid.ToString();
            var channel = $"chatbot-ai-{consumerId}-chatbot";
            var timestamp = DateTime.Now.ToString("HH:mm dd/MM/yyyy");
        
            await _firebaseService.SendToUserChannelAsync(channel, $"[{timestamp}] User: {request.Question}");
        
            var aiResponse = await _aiService.AskAsync(request.Question);
        
            timestamp = DateTime.Now.ToString("HH:mm dd/MM/yyyy");
        
            await _firebaseService.SendToUserChannelAsync(channel, $"[{timestamp}] AI: {aiResponse}");

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