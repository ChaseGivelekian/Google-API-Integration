using System.Text.Json;

namespace Google_API_Integration.Utils;

public static class ConfigurationHelper
{
    public static string GetGeminiApiKey()
    {
        var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (!string.IsNullOrEmpty(apiKey))
        {
            return apiKey;
        }

        try
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            if (File.Exists(configPath))
            {
                var jsonContent = File.ReadAllText(configPath);
                var config = JsonSerializer.Deserialize<JsonElement>(jsonContent);
                if (config.TryGetProperty("GeminiApiKey", out var keyElement))
                {
                    return keyElement.GetString() ?? "";
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error reading configuration: {e.Message}");
        }

        return "";
    }
}