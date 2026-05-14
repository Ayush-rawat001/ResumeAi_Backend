namespace AIService.DTOs
{
    public class AiRequestDto
    {
        public int ResumeId { get; set; }
        public string Input { get; set; } = string.Empty;
    }

    public class AiAdminStatsDto
    {
        public int TotalRequests { get; set; }
        public int TotalTokensUsed { get; set; }
        public int CompletedRequests { get; set; }
        public int FailedRequests { get; set; }
        public Dictionary<string, int> RequestsByModel { get; set; } = new();
        public Dictionary<string, int> RequestsByType { get; set; } = new();
        public Dictionary<string, int> RequestsByStatus { get; set; } = new();
        public List<UserAiUsageDto> TopUsersByUsage { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class UserAiUsageDto
    {
        public int UserId { get; set; }
        public int TotalRequests { get; set; }
        public int TotalTokens { get; set; }
        public string TopModel { get; set; } = string.Empty;
    }
}
