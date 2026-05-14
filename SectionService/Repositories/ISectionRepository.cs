using SectionService.Models;

namespace SectionService.Repositories
{
    public interface ISectionRepository
    {
        Task<List<ResumeSection>> FindByResumeId(int resumeId);
        Task<ResumeSection?> FindBySectionId(int sectionId);
        Task<List<ResumeSection>> FindByResumeIdAndSectionType(int resumeId, SectionType type);
        Task<List<ResumeSection>> FindByResumeIdOrderByDisplayOrder(int resumeId);
        Task<List<ResumeSection>> FindByAiGenerated(bool aiGenerated);
        Task<int> CountByResumeId(int resumeId);
        Task<int> GetMaxDisplayOrder(int resumeId);
        Task Add(ResumeSection section);
        Task Update(ResumeSection section);
        Task DeleteByResumeId(int resumeId);
        Task DeleteBySectionId(int sectionId);
        Task SaveChanges();
    }
}
