using Google.GenAI;
using RentMaster.Ai.Interfaces;

namespace RentMaster.Ai.Services
{
    public class GoogleAiService : IGoogleAiService, IDisposable
    {
        private readonly Client _client;
        private bool _disposed;

        public GoogleAiService()
        {
            string apiKey = Environment.GetEnvironmentVariable("GERMINI_AI_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentException("GERMINI_AI_API_KEY environment variable is required");
            _client = new Client(apiKey: apiKey);
        }

        public async Task<string> AskAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
                throw new ArgumentException("Prompt is required.", nameof(prompt));

            string model = "gemini-2.5-flash";
            int maxRetries = 3;        
            int timeoutSeconds = 20;   

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    var task = _client.Models.GenerateContentAsync(
                        model: model,
                        contents: prompt
                    );

                    var completedTask = await Task.WhenAny(task, Task.Delay(TimeSpan.FromSeconds(timeoutSeconds)));

                    if (completedTask != task)
                    {
                        Console.WriteLine($"Attempt {attempt} timed out after {timeoutSeconds}s.");
                        if (attempt == maxRetries)
                            return $"Request timed out after {maxRetries} attempts";
                        continue; // retry
                    }

                    var response = await task;

                    if (response.Candidates.Count == 0 || response.Candidates[0].Content?.Parts.Count == 0)
                    {
                        Console.WriteLine($"Attempt {attempt}: No text returned.");
                        if (attempt == maxRetries)
                            return "No response from AI after maximum attempts";
                        continue;
                    }

                    string text = response.Candidates[0].Content.Parts[0].Text ?? "No text response";
                    Console.WriteLine($"Attempt {attempt}: Received text.");
                    return text;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Attempt {attempt} error: {ex.Message}");
                    if (attempt == maxRetries)
                        return $"Error after {maxRetries} attempts: {ex.Message}";
                }
            }

            return "Unknown error";
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }
}
