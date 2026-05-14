using System.Text.Json.Serialization;

namespace JobSearchService.DTOs
{
    public class AnalyzeJobFitRequest
    {
        public int ResumeId { get; set; }
        public string Skills { get; set; } = string.Empty;
    }

    public class JobSearchResultDto
    {
        public string Title { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int MatchScore { get; set; }
        public List<string> MatchingSkills { get; set; } = new();
        public List<string> MissingSkills { get; set; } = new();
    }

    public class HimalayasJobResponse
    {
        [JsonPropertyName("jobs")]
        public List<HimalayasJob> Jobs { get; set; } = new();
    }

    public class HimalayasJob
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("excerpt")]
        public string? Excerpt { get; set; }

        [JsonPropertyName("companyName")]
        public string CompanyName { get; set; } = string.Empty;

        [JsonPropertyName("applicationLink")]
        public string ApplicationLink { get; set; } = string.Empty;

        [JsonPropertyName("location")]
        public string? Location { get; set; }

        [JsonPropertyName("skills")]
        public List<string>? Skills { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;

        public static ApiResponse<T> Ok(T data, string message = "") => new() { Success = true, Data = data, Message = message };
        public static ApiResponse<T> Fail(string message) => new() { Success = false, Message = message };
    }
}
