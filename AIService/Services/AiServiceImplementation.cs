using AIService.DTOs;
using AIService.Entities;
using AIService.Providers;
using AIService.Repositories;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;

namespace AIService.Services
{
    public class AiServiceImplementation : IAiService
    {
        private readonly IAiRequestRepository _repository;
        private readonly IAiProvider _aiProvider;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AiServiceImplementation> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private const int FREE_QUOTA = 10;
        private const int PREMIUM_QUOTA = 100;

        public AiServiceImplementation(
            IAiRequestRepository repository,
            IAiProvider aiProvider,
            IMemoryCache cache,
            ILogger<AiServiceImplementation> logger,
            IHttpContextAccessor httpContextAccessor)
        {
            _repository = repository;
            _aiProvider = aiProvider;
            _cache = cache;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResponse<string>> GenerateSummary(int userId, AiRequestDto dto)
            => await ProcessAiRequest(userId, dto, "SUMMARY",
                $"Generate a professional resume summary for: {dto.Input}");

        public async Task<ApiResponse<string>> GenerateBulletPoints(int userId, AiRequestDto dto)
            => await ProcessAiRequest(userId, dto, "BULLETS",
                $"Generate 3 strong resume bullet points for: {dto.Input}");

        public async Task<ApiResponse<string>> ImproveSection(int userId, AiRequestDto dto)
            => await ProcessAiRequest(userId, dto, "IMPROVE",
                $"Improve this resume content professionally: {dto.Input}");

        public async Task<ApiResponse<string>> CheckAts(int userId, AiRequestDto dto)
            => await ProcessAiRequest(userId, dto, "ATS",
                $"Analyze ATS score and missing keywords for: {dto.Input}");

        public async Task<ApiResponse<string>> SuggestSkills(int userId, AiRequestDto dto)
            => await ProcessAiRequest(userId, dto, "SKILLS",
                $"Suggest relevant skills for job role: {dto.Input}");

        public async Task<ApiResponse<List<AiRequest>>> GetHistory(int userId)
        {
            var history = await _repository.FindByUserId(userId);
            return ApiResponse<List<AiRequest>>.Ok(history);
        }

        public async Task<ApiResponse<int>> GetRemainingQuota(int userId)
        {
            int used = GetUsedQuota(userId);
            int limit = GetQuotaLimit();
            return await Task.FromResult(ApiResponse<int>.Ok(Math.Max(0, limit - used)));
        }

        // ── Admin: Aggregated AI Usage Stats ───────────────────────────────────
        public async Task<ApiResponse<AiAdminStatsDto>> GetAdminStats()
        {
            var totalRequests = await _repository.GetTotalRequestCount();
            var totalTokens   = await _repository.GetTotalTokensUsed();
            var byModel       = await _repository.GetCountByModel();
            var byType        = await _repository.GetCountByType();
            var byStatus      = await _repository.GetCountByStatus();
            var perUser       = await _repository.GetQuotaUsageByUser();

            var stats = new AiAdminStatsDto
            {
                TotalRequests      = totalRequests,
                TotalTokensUsed    = totalTokens,
                CompletedRequests  = byStatus.GetValueOrDefault("COMPLETED", 0),
                FailedRequests     = byStatus.GetValueOrDefault("FAILED", 0),
                RequestsByModel    = byModel,
                RequestsByType     = byType,
                RequestsByStatus   = byStatus,
                TopUsersByUsage    = perUser.Take(20).Select(u => new UserAiUsageDto
                {
                    UserId        = u.UserId,
                    TotalRequests = u.TotalRequests,
                    TotalTokens   = u.TotalTokens,
                    TopModel      = u.TopModel
                }).ToList(),
                GeneratedAt = DateTime.UtcNow
            };

            return ApiResponse<AiAdminStatsDto>.Ok(stats, "AI usage stats fetched successfully");
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private async Task<ApiResponse<string>> ProcessAiRequest(
            int userId, AiRequestDto dto, string type, string prompt)
        {
            if (string.IsNullOrWhiteSpace(dto.Input))
                return ApiResponse<string>.Fail("Input cannot be empty");

            dto.Input = dto.Input.Trim();

            if (!CheckQuota(userId))
                return ApiResponse<string>.Fail("Monthly quota exceeded. Upgrade to Premium for more requests.");

            var aiRequest = new AiRequest
            {
                UserId      = userId,
                ResumeId    = dto.ResumeId,
                RequestType = type,
                InputPrompt = prompt,
                Status      = "QUEUED"
            };

            await _repository.Add(aiRequest);
            await _repository.SaveChanges();

            try
            {
                string responseText = await _aiProvider.GenerateAsync(prompt);

                aiRequest.AiResponse  = responseText;
                aiRequest.Status      = "COMPLETED";
                aiRequest.CompletedAt = DateTime.UtcNow;
                aiRequest.TokensUsed  = responseText.Length / 4;

                IncrementQuota(userId);
                await _repository.SaveChanges();

                return ApiResponse<string>.Ok(responseText);
            }
            catch (Exception ex)
            {
                aiRequest.Status = "FAILED";
                await _repository.SaveChanges();
                _logger.LogError(ex, "AI generation failed for type={Type}", type);
                return ApiResponse<string>.Fail("AI generation failed.");
            }
        }

        private bool CheckQuota(int userId)
            => GetUsedQuota(userId) < GetQuotaLimit();

        private int GetQuotaLimit()
        {
            var plan = _httpContextAccessor.HttpContext?.User
                ?.FindFirst("SubscriptionPlan")?.Value;
            return plan == "PREMIUM" ? PREMIUM_QUOTA : FREE_QUOTA;
        }

        private int GetUsedQuota(int userId)
        {
            string key = $"ai-quota-{userId}-{DateTime.UtcNow:yyyy-MM}";
            return _cache.Get<int?>(key) ?? 0;
        }

        private void IncrementQuota(int userId)
        {
            string key    = $"ai-quota-{userId}-{DateTime.UtcNow:yyyy-MM}";
            int current   = GetUsedQuota(userId);
            _cache.Set(key, current + 1, TimeSpan.FromDays(31));
        }
    }
}
