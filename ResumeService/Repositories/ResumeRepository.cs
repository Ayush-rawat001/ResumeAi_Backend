using Microsoft.EntityFrameworkCore;
using ResumeService.Data;
using ResumeService.Models;

namespace ResumeService.Repositories
{
    public class ResumeRepository : IResumeRepository
    {
        private readonly AppDbContext _context;

        public ResumeRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Resume>> FindByUserId(int userId)
            => await _context.Resumes.Where(r => r.UserId == userId).ToListAsync();

        public async Task<Resume?> FindByResumeId(int resumeId)
            => await _context.Resumes.FindAsync(resumeId);

        public async Task<List<Resume>> FindByStatus(string status)
            => await _context.Resumes.Where(r => r.Status == status).ToListAsync();

        public async Task<List<Resume>> FindByTargetJobTitle(string title)
            => await _context.Resumes.Where(r => r.TargetJobTitle.Contains(title)).ToListAsync();

        public async Task<List<Resume>> FindByIsPublic(bool isPublic)
            => await _context.Resumes.Where(r => r.IsPublic == isPublic).ToListAsync();

        public async Task<int> CountByUserId(int userId)
            => await _context.Resumes.CountAsync(r => r.UserId == userId);

        public async Task<List<Resume>> FindByTemplateId(int templateId)
            => await _context.Resumes.Where(r => r.TemplateId == templateId).ToListAsync();

        public async Task Add(Resume resume)
            => await _context.Resumes.AddAsync(resume);

        public async Task Update(Resume resume)
        {
            _context.Resumes.Update(resume);
            await Task.CompletedTask;
        }

        public async Task DeleteByResumeId(int resumeId)
        {
            var resume = await _context.Resumes.FindAsync(resumeId);
            if (resume != null) _context.Resumes.Remove(resume);
        }

        public async Task UpdateAtsScoreAsync(int resumeId, int score)
        {
            await _context.Resumes
                .Where(r => r.ResumeId == resumeId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.AtsScore, score)
                    .SetProperty(r => r.UpdatedAt, DateTime.UtcNow));
        }

        public async Task IncrementViewCountAsync(int resumeId)
        {
            await _context.Resumes
                .Where(r => r.ResumeId == resumeId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.ViewCount, r => r.ViewCount + 1));
        }

        public async Task SaveChanges()
            => await _context.SaveChangesAsync();

        // ── Admin Analytics ───────────────────────────────────────────────────

        public async Task<int> GetTotalResumeCount()
            => await _context.Resumes.CountAsync();

        public async Task<int> GetPublicResumeCount()
            => await _context.Resumes.CountAsync(r => r.IsPublic);

        public async Task<int> GetTotalViewCount()
            => await _context.Resumes.SumAsync(r => r.ViewCount);

        public async Task<Dictionary<int, int>> GetTemplateUsageStats()
            => await _context.Resumes
                .GroupBy(r => r.TemplateId)
                .Select(g => new { TemplateId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.TemplateId, x => x.Count);
    }
}
