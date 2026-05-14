using JobSearchService.DTOs;
using JobSearchService.Models;

namespace JobSearchService.Services
{
    public interface IJobSearchService
    {
        Task<ApiResponse<List<JobSearchResultDto>>> SearchAndMatchJobs(int userId, AnalyzeJobFitRequest request);
        Task<ApiResponse<List<JobMatch>>> GetSavedMatches(int userId);
        Task<ApiResponse<bool>> BookmarkMatch(int matchId);
    }
}
