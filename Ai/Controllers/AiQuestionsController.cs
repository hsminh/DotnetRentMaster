using Microsoft.AspNetCore.Mvc;
using RentMaster.Ai.Interfaces;

namespace RentMaster.Ai.Controllers;

[ApiController]
[Route("public/api/ai/[controller]")]
public class QuestionsController : ControllerBase
{
    private readonly IGoogleAiService _googleAiService;
    private readonly ILogger<QuestionsController> _logger;

    public QuestionsController(
        IGoogleAiService googleAiService,
        ILogger<QuestionsController> logger)
    {
        _googleAiService = googleAiService;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(AiQuestionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateQuestion([FromBody] CreateAiQuestionRequest request, CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Question))
        {
            Console.WriteLine("Received empty or null question."); 
            return BadRequest(new { error = "Question is required." });
        }

        Console.WriteLine($"Received question: {request.Question}"); 

        try
        {
            var answer = await _googleAiService.AskAsync(
                request.Question
            );

            Console.WriteLine($"AI Answer: {answer}"); 

            return Ok(new AiQuestionResponse
            {
                Question = request.Question,
                Answer = answer,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Request was cancelled by the client.");
            _logger.LogWarning("AI request was cancelled.");
            return StatusCode(499, new { error = "Client cancelled the request." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing AI question: {ex.Message}");
            _logger.LogError(ex, "Error processing AI question.");
            return StatusCode(500, new { error = ex.Message });
        }
    }

}
