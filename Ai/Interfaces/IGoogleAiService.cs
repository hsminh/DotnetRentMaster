namespace RentMaster.Ai.Interfaces;

public interface IGoogleAiService
{
    Task<string> AskAsync(
        string prompt
    );
}
