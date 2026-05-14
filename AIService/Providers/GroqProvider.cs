using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AIService.Providers
{
    public class GroqProvider : IAiProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GroqProvider> _logger;

        public GroqProvider(HttpClient httpClient, IConfiguration configuration, ILogger<GroqProvider> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Groq:ApiKey"] ?? throw new ArgumentNullException("Groq ApiKey is missing");
            _logger = logger;
        }

        public async Task<string> GenerateAsync(string prompt)
        {
            try
            {
                var url = "https://api.groq.com/openai/v1/chat/completions";

                var requestBody = new
                {
                    model = "llama-3.1-8b-instant",
                    messages = new[]
                    {
                        new { role = "user", content = prompt }
                    },
                    temperature = 0.7
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                var response = await _httpClient.PostAsync(url, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorJson = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Groq API Error Response: {Error}", errorJson);
                    throw new Exception($"AI Generation failed: {response.StatusCode}");
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GroqResponse>(responseJson);

                return result?.Choices?[0].Message?.Content ?? "No response from AI";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Groq API Error");
                throw new Exception("AI Generation failed. Please try again later.");
            }
        }

        private class GroqResponse
        {
            [JsonPropertyName("choices")]
            public List<Choice>? Choices { get; set; }
        }

        private class Choice
        {
            [JsonPropertyName("message")]
            public Message? Message { get; set; }
        }

        private class Message
        {
            [JsonPropertyName("content")]
            public string? Content { get; set; }
        }
    }
}
