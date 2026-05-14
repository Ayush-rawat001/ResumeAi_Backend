using SectionService.Models;
using SectionService.Repositories;
using SectionService.DTOs;
using Microsoft.Extensions.Logging;

namespace SectionService.Services
{
    public class SectionServiceImplementation : ISectionService
    {
        private readonly ISectionRepository _repository;
        private readonly ILogger<SectionServiceImplementation> _logger;

        public SectionServiceImplementation(ISectionRepository repository, ILogger<SectionServiceImplementation> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ResumeSection> AddSection(ResumeSection section)
        {
            try
            {
                section.CreatedAt = DateTime.UtcNow;
                section.UpdatedAt = DateTime.UtcNow;
                section.IsVisible = true;

                // Handle DisplayOrder: Max existing + 1
                int maxOrder = await _repository.GetMaxDisplayOrder(section.ResumeId);
                section.DisplayOrder = maxOrder + 1;

                await _repository.Add(section);
                await _repository.SaveChanges();
                return section;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding section to resume {ResumeId}", section.ResumeId);
                throw;
            }
        }

        public async Task<List<ResumeSection>> GetSectionsByResume(int resumeId)
        {
            return await _repository.FindByResumeIdOrderByDisplayOrder(resumeId);
        }

        public async Task<ResumeSection?> GetSectionById(int id)
        {
            return await _repository.FindBySectionId(id);
        }

        public async Task<bool> ReorderSections(int resumeId, List<int> orderedSectionIds)
        {
            try
            {
                var sections = await _repository.FindByResumeId(resumeId);
                var sectionMap = sections.ToDictionary(s => s.SectionId);

                if (orderedSectionIds.Any(id => !sectionMap.ContainsKey(id)))
                {
                    return false;
                }

                int order = 1;
                foreach (var id in orderedSectionIds)
                {
                    var section = sectionMap[id];
                    section.DisplayOrder = order++;
                    section.UpdatedAt = DateTime.UtcNow;
                }

                await _repository.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering sections for resume {ResumeId}", resumeId);
                throw;
            }
        }

        public async Task<bool> ToggleVisibility(int sectionId)
        {
            try
            {
                var section = await _repository.FindBySectionId(sectionId);
                if (section == null) return false;

                section.IsVisible = !section.IsVisible;
                section.UpdatedAt = DateTime.UtcNow;

                await _repository.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling visibility for section {Id}", sectionId);
                throw;
            }
        }

        public async Task<bool> DeleteAllSections(int resumeId)
        {
            try
            {
                await _repository.DeleteByResumeId(resumeId);
                await _repository.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting all sections for resume {ResumeId}", resumeId);
                throw;
            }
        }

        public async Task<List<ResumeSection>> GetSectionsByType(int resumeId, string type)
        {
            if (Enum.TryParse<SectionType>(type, true, out var sectionType))
            {
                return await _repository.FindByResumeIdAndSectionType(resumeId, sectionType);
            }
            return new List<ResumeSection>();
        }

        public async Task<bool> BulkUpdateSections(int resumeId, List<ResumeSection> sections)
        {
            try
            {
                var existingSections = await _repository.FindByResumeId(resumeId);
                var existingMap = existingSections.ToDictionary(s => s.SectionId);

                foreach (var updatedSection in sections)
                {
                    if (existingMap.TryGetValue(updatedSection.SectionId, out var existing))
                    {
                        existing.Title = updatedSection.Title;
                        existing.Content = updatedSection.Content;
                        existing.IsVisible = updatedSection.IsVisible;
                        existing.DisplayOrder = updatedSection.DisplayOrder;
                        existing.UpdatedAt = DateTime.UtcNow;
                    }
                }

                await _repository.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk updating sections for resume {ResumeId}", resumeId);
                throw;
            }
        }

        public async Task<bool> DeleteSectionAsync(int sectionId)
        {
            var existing = await _repository.FindBySectionId(sectionId);
            if (existing == null) return false;

            await _repository.DeleteBySectionId(sectionId);
            await _repository.SaveChanges();
            return true;
        }

        public async Task<SectionDto?> UpdateSectionAsync(int sectionId, UpdateSectionDto dto)
        {
            var existing = await _repository.FindBySectionId(sectionId);
            if (existing == null) return null;

            if (dto.Title != null) existing.Title = dto.Title;
            if (dto.Content != null) existing.Content = dto.Content;
            if (dto.DisplayOrder.HasValue) existing.DisplayOrder = dto.DisplayOrder.Value;

            existing.UpdatedAt = DateTime.UtcNow;

            await _repository.Update(existing);
            await _repository.SaveChanges();

            return new SectionDto
            {
                SectionId = existing.SectionId,
                ResumeId = existing.ResumeId,
                SectionType = existing.SectionType,
                Title = existing.Title,
                Content = existing.Content,
                DisplayOrder = existing.DisplayOrder,
                IsVisible = existing.IsVisible,
                AiGenerated = existing.AiGenerated,
                CreatedAt = existing.CreatedAt,
                UpdatedAt = existing.UpdatedAt
            };
        }
    }
}
