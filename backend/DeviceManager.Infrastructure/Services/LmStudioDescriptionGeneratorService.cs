using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using DeviceManager.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DeviceManager.Infrastructure.Services;

public class LmStudioDescriptionGeneratorService(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<LmStudioDescriptionGeneratorService> logger)
    : IDescriptionGeneratorService
{
    // Sensible defaults — override in .env or appsettings
    private string BaseUrl => configuration["LmStudio:BaseUrl"] ?? "http://localhost:1234";
    private string Model => configuration["LmStudio:Model"] ?? "local-model";

    public async Task<string?> GenerateAsync(
        string name,
        string manufacturer,
        string type,
        string operatingSystem,
        string processor,
        int ramAmount)
    {
        var prompt = BuildPrompt(name, manufacturer, type, operatingSystem, processor, ramAmount);

        var requestBody = new
        {
            model = Model,
            messages = new[]
            {
                new
                {
                    role = "system",
                    content = "You are an assistant that generates short, human-friendly device descriptions. " +
                              "Summarize the device based on its specifications in one concise sentence. " +
                              "Focus on what the device is suitable for (e.g., business, daily use, performance). " +
                              "Do not list all specifications explicitly. " +
                              "Use natural, simple language."
                },
                new { role = "user", content = prompt }
            },
            temperature = 0.4, // low temp = consistent, factual output
            max_tokens = 80
        };

        try
        {
            var client = httpClientFactory.CreateClient("LmStudio");
            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{BaseUrl}/v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "LM Studio returned {StatusCode} for description generation.",
                    response.StatusCode);
                return null;
            }

            using var doc = await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync());
            var generated = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return generated?.Trim();
        }
        catch (Exception ex)
        {
            // LM Studio not running — fail gracefully, don't crash the request
            logger.LogWarning(ex, "LM Studio unreachable. Skipping description generation.");
            return null;
        }
    }

    private static string BuildPrompt(
        string name, string manufacturer, string type,
        string operatingSystem, string processor, int ramAmount) =>
        $"Device details: Name={name}; Manufacturer={manufacturer}; Type={type}; " +
        $"OS={operatingSystem}; CPU={processor}; RAM={ramAmount}GB. " +
        $"Generate a short, user-friendly description.";
}