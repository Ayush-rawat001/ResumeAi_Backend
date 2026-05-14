using ResumeService.Models;

namespace ResumeService.Repositories
{
    public interface IResumeRepository
    {
        Task<List<Resume>> FindByUserId(int userId);
        Task<Resume?> FindByResumeId(int resumeId);
        Task<List<Resume>> FindByStatus(string status);
        Task<List<Resume>> FindByTargetJobTitle(string title);
        Task<List<Resume>> FindByIsPublic(bool isPublic);
        Task<int> CountByUserId(int userId);
        Task<List<Resume>> FindByTemplateId(int templateId);
        Task Add(Resume resume);
        Task Update(Resume resume);
        Task DeleteByResumeId(int resumeId);
        Task UpdateAtsScoreAsync(int resumeId, int score);
        Task IncrementViewCountAsync(int resumeId);
        Task SaveChanges();

        // Admin Analytics
        Task<int> GetTotalResumeCount();
        Task<int> GetPublicResumeCount();
        Task<int> GetTotalViewCount();
        Task<Dictionary<int, int>> GetTemplateUsageStats();
    }
}
