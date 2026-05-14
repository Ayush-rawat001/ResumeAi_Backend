using AIService.Entities;

namespace AIService.Repositories
{
    public interface IAiRequestRepository
    {
        Task Add(AiRequest request);
        Task<List<AiRequest>> FindByUserId(int userId);
        Task<List<AiRequest>> FindByResumeId(int resumeId);
        Task SaveChanges();

        // Admin stats
        Task<int> GetTotalRequestCount();
        Task<int> GetTotalTokensUsed();
        Task<Dictionary<string, int>> GetCountByModel();
        Task<Dictionary<string, int>> GetCountByType();
        Task<Dictionary<string, int>> GetCountByStatus();
        Task<List<UserQuotaDto>> GetQuotaUsageByUser();
    }

    public class UserQuotaDto
    {
        public int UserId { get; set; }
        public int TotalRequests { get; set; }
        public int TotalTokens { get; set; }
        public string TopModel { get; set; } = string.Empty;
    }
}
