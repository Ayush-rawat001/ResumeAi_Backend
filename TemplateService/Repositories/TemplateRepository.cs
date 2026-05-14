using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TemplateService.Data;
using TemplateService.Models;

namespace TemplateService.Repositories
{
    public class TemplateRepository : ITemplateRepository
    {
        private readonly TemplateDbContext _context;

        public TemplateRepository(TemplateDbContext context)
        {
            _context = context;
        }

        public async Task<ResumeTemplate?> GetByIdAsync(int id)
        {
            return await _context.ResumeTemplates.FindAsync(id);
        }

        public async Task<List<ResumeTemplate>> GetAllAsync()
        {
            return await _context.ResumeTemplates.ToListAsync();
        }

        public async Task<List<ResumeTemplate>> GetByCategoryAsync(string category)
        {
            return await _context.ResumeTemplates
                .Where(t => t.Category == category)
                .ToListAsync();
        }

        public async Task<List<ResumeTemplate>> GetFreeTemplatesAsync()
        {
            return await _context.ResumeTemplates
                .Where(t => !t.IsPremium)
                .ToListAsync();
        }

        public async Task<List<ResumeTemplate>> GetPremiumTemplatesAsync()
        {
            return await _context.ResumeTemplates
                .Where(t => t.IsPremium)
                .ToListAsync();
        }

        public async Task<List<ResumeTemplate>> GetPopularTemplatesAsync()
        {
            return await _context.ResumeTemplates
                .OrderByDescending(t => t.UsageCount)
                .Take(10)
                .ToListAsync();
        }

        public async Task AddAsync(ResumeTemplate template)
        {
            await _context.ResumeTemplates.AddAsync(template);
        }

        public Task UpdateAsync(ResumeTemplate template)
        {
            _context.ResumeTemplates.Update(template);
            return Task.CompletedTask;
        }

        public async Task IncrementUsageCountAsync(int id)
        {
            await _context.ResumeTemplates
                .Where(t => t.TemplateId == id)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.UsageCount, t => t.UsageCount + 1));
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
