namespace RentMaster.Ai.Interfaces;

public class CreateAiQuestionRequest
{
    public string Question { get; set; } = string.Empty;
}

public class AiQuestionResponse
{
    public string Question { get; set; } = string.Empty;

    public string Answer { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }
}