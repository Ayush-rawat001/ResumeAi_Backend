using Microsoft.EntityFrameworkCore;
using SectionService.Data;
using SectionService.Models;

namespace SectionService.Repositories
{
    public class SectionRepository : ISectionRepository
    {
        private readonly AppDbContext _context;

        public SectionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ResumeSection>> FindByResumeId(int resumeId)
        {
            return await _context.ResumeSections
                .Where(s => s.ResumeId == resumeId)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();
        }

        public async Task<ResumeSection?> FindBySectionId(int sectionId)
        {
            return await _context.ResumeSections.FindAsync(sectionId);
        }

        public async Task<List<ResumeSection>> FindByResumeIdAndSectionType(int resumeId, SectionType type)
        {
            return await _context.ResumeSections
                .Where(s => s.ResumeId == resumeId && s.SectionType == type)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<ResumeSection>> FindByResumeIdOrderByDisplayOrder(int resumeId)
        {
            return await _context.ResumeSections
                .Where(s => s.ResumeId == resumeId)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<ResumeSection>> FindByAiGenerated(bool aiGenerated)
        {
            return await _context.ResumeSections
                .Where(s => s.AiGenerated == aiGenerated)
                .ToListAsync();
        }

        public async Task<int> CountByResumeId(int resumeId)
        {
            return await _context.ResumeSections.CountAsync(s => s.ResumeId == resumeId);
        }

        public async Task<int> GetMaxDisplayOrder(int resumeId)
        {
            var max = await _context.ResumeSections
                .Where(s => s.ResumeId == resumeId)
                .MaxAsync(s => (int?)s.DisplayOrder) ?? 0;
            return max;
        }

        public async Task Add(ResumeSection section)
        {
            await _context.ResumeSections.AddAsync(section);
        }

        public async Task Update(ResumeSection section)
        {
            _context.ResumeSections.Update(section);
            await Task.CompletedTask;
        }

        public async Task DeleteByResumeId(int resumeId)
        {
            var sections = await _context.ResumeSections.Where(s => s.ResumeId == resumeId).ToListAsync();
            _context.ResumeSections.RemoveRange(sections);
        }

        public async Task DeleteBySectionId(int sectionId)
        {
            var section = await _context.ResumeSections.FindAsync(sectionId);
            if (section != null)
            {
                _context.ResumeSections.Remove(section);
            }
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}
