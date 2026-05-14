using AIService.DTOs;
using AIService.Entities;

namespace AIService.Services
{
    public interface IAiService
    {
        Task<ApiResponse<string>> GenerateSummary(int userId, AiRequestDto dto);
        Task<ApiResponse<string>> GenerateBulletPoints(int userId, AiRequestDto dto);
        Task<ApiResponse<string>> ImproveSection(int userId, AiRequestDto dto);
        Task<ApiResponse<string>> CheckAts(int userId, AiRequestDto dto);
        Task<ApiResponse<string>> SuggestSkills(int userId, AiRequestDto dto);
        Task<ApiResponse<List<AiRequest>>> GetHistory(int userId);
        Task<ApiResponse<int>> GetRemainingQuota(int userId);

        // Admin-only
        Task<ApiResponse<AiAdminStatsDto>> GetAdminStats();
    }
}
