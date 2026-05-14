using Microsoft.EntityFrameworkCore;
using AIService.Data;
using AIService.Entities;

namespace AIService.Repositories
{
    public class AiRequestRepository : IAiRequestRepository
    {
        private readonly AiDbContext _context;

        public AiRequestRepository(AiDbContext context)
        {
            _context = context;
        }

        public async Task Add(AiRequest request)
            => await _context.AiRequests.AddAsync(request);

        public async Task<List<AiRequest>> FindByUserId(int userId)
            => await _context.AiRequests
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

        public async Task<List<AiRequest>> FindByResumeId(int resumeId)
            => await _context.AiRequests
                .Where(r => r.ResumeId == resumeId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

        public async Task SaveChanges()
            => await _context.SaveChangesAsync();

        // ── Admin stats ────────────────────────────────────────────────────────

        public async Task<int> GetTotalRequestCount()
            => await _context.AiRequests.CountAsync();

        public async Task<int> GetTotalTokensUsed()
            => await _context.AiRequests.SumAsync(r => (int?)r.TokensUsed) ?? 0;

        public async Task<Dictionary<string, int>> GetCountByModel()
            => await _context.AiRequests
                .GroupBy(r => r.Model)
                .Select(g => new { Model = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Model, x => x.Count);

        public async Task<Dictionary<string, int>> GetCountByType()
            => await _context.AiRequests
                .GroupBy(r => r.RequestType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count);

        public async Task<Dictionary<string, int>> GetCountByStatus()
            => await _context.AiRequests
                .GroupBy(r => r.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

        public async Task<List<UserQuotaDto>> GetQuotaUsageByUser()
            => await _context.AiRequests
                .GroupBy(r => r.UserId)
                .Select(g => new UserQuotaDto
                {
                    UserId        = g.Key,
                    TotalRequests = g.Count(),
                    TotalTokens   = g.Sum(r => r.TokensUsed),
                    TopModel      = g.GroupBy(r => r.Model)
                                     .OrderByDescending(mg => mg.Count())
                                     .Select(mg => mg.Key)
                                     .FirstOrDefault() ?? "N/A"
                })
                .OrderByDescending(u => u.TotalRequests)
                .ToListAsync();
    }
}
