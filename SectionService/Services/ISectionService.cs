using SectionService.Models;
using SectionService.DTOs;

namespace SectionService.Services
{
    public interface ISectionService
    {
        Task<ResumeSection> AddSection(ResumeSection section);
        Task<List<ResumeSection>> GetSectionsByResume(int resumeId);
        Task<ResumeSection?> GetSectionById(int id);
        Task<bool> ReorderSections(int resumeId, List<int> orderedSectionIds);
        Task<bool> ToggleVisibility(int sectionId);
        Task<bool> DeleteAllSections(int resumeId);
        Task<List<ResumeSection>> GetSectionsByType(int resumeId, string type);
        Task<bool> BulkUpdateSections(int resumeId, List<ResumeSection> sections);

        // New Minimal Endpoints for Frontend
        Task<bool> DeleteSectionAsync(int sectionId);
        Task<SectionDto?> UpdateSectionAsync(int sectionId, UpdateSectionDto dto);
    }
}
