using System.Text;
using System.Text.Json;
using Google_API_Integration.Interfaces;

namespace Google_API_Integration.Services.Gemini;

public class GeminiService(string apiKey) : IGeminiService
{
    private readonly HttpClient _httpClient = new();
    private readonly string _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));

    private const string ApiBaseUrl =
        "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

    public async Task<string> GenerateContentAsync(string prompt)
    {
        var requestContent = new
        {
            system_instruction = new
            {
                parts = new[]
                {
                    new { text = "Don't use asterisks in your response." }
                }
            },
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        return await SendRequestAsync(requestContent);
    }

    public async Task<string> AnalyzeDocumentContentAsync(string documentContent)
    {
        var prompt = $"Analyze the following document content: {documentContent}";
        return await GenerateContentAsync(prompt);
    }

    public async Task<string> SummarizeSubmissionAsync(string submissionContent, string assignmentDescription)
    {
        var prompt =
            $"Assignment description: {assignmentDescription}\n\nSubmission content: {submissionContent}\n\nPlease analyze this submission in relation to the assignment requirements and provide feedback.";
        return await GenerateContentAsync(prompt);
    }

    /// <summary>
    /// Sends a request to the Gemini API and returns the response.
    /// </summary>
    /// <param name="requestContent">Object that is in the Gemini REST format</param>
    /// <returns>Returns a string of the AI's response</returns>
    private async Task<string> SendRequestAsync(object requestContent)
    {
        var url = $"{ApiBaseUrl}?key={_apiKey}";

        var requestJson = JsonSerializer.Serialize(requestContent);
        var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, httpContent);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

        // Extract the text from the response
        if (responseObj.TryGetProperty("candidates", out var candidates) &&
            candidates.GetArrayLength() > 0 &&
            candidates[0].TryGetProperty("content", out var content) &&
            content.TryGetProperty("parts", out var parts) &&
            parts.GetArrayLength() > 0 &&
            parts[0].TryGetProperty("text", out var text))
        {
            return text.GetString() ?? string.Empty;
        }

        return string.Empty;
    }
}