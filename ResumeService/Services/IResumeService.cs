using ResumeService.Models;
using ResumeService.DTOs;

namespace ResumeService.Services
{
    public interface IResumeService
    {
        Task<Resume> CreateResume(Resume resume);
        Task<object?> GetResumeById(int id, string? token = null);
        Task<List<Resume>> GetResumesByUser(int userId);
        Task<Resume?> DuplicateResume(int id, string? token = null);
        Task<bool> UpdateAtsScore(int id, int score);
        Task<bool> PublishResume(int id);
        Task<bool> UnpublishResume(int id);
        Task<List<Resume>> GetPublicResumes();
        Task<bool> IncrementViewCount(int id);
        Task<List<Resume>> GetResumesByTemplate(int templateId);

        // Minimal Endpoints
        Task<bool> DeleteResumeAsync(int id, int userId, string role);
        Task<ResumeDto?> UpdateResumeAsync(int id, UpdateResumeDto dto, int userId);

        // Admin Analytics
        Task<ResumeAnalyticsDto> GetAdminAnalytics();
    }
}
