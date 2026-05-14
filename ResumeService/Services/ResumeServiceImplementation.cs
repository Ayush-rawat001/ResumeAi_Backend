using ResumeService.Models;
using ResumeService.Repositories;
using ResumeService.DTOs;
using System.Net.Http.Json;

namespace ResumeService.Services
{
    public class ResumeServiceImplementation : IResumeService
    {
        private readonly IResumeRepository _repository;
        private readonly ILogger<ResumeServiceImplementation> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ResumeServiceImplementation(IResumeRepository repository, ILogger<ResumeServiceImplementation> logger, IHttpClientFactory httpClientFactory)
        {
            _repository = repository;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Resume> CreateResume(Resume resume)
        {
            try
            {
                resume.CreatedAt = DateTime.UtcNow;
                resume.UpdatedAt = DateTime.UtcNow;
                resume.Status = "DRAFT";
                resume.AtsScore = 0;
                resume.IsPublic = false;
                resume.ViewCount = 0;

                await _repository.Add(resume);
                await _repository.SaveChanges();
                return resume;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating resume");
                throw;
            }
        }

        public async Task<object?> GetResumeById(int id, string? token = null)
        {
            var resume = await _repository.FindByResumeId(id);
            if (resume == null) return null;

            object[] sections = Array.Empty<object>();

            try
            {
                var client = _httpClientFactory.CreateClient("SectionService");
                
                // Forward the token if provided
                if (!string.IsNullOrEmpty(token))
                {
                    if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Substring(7).Trim());
                    }
                    else
                    {
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    }
                }

                var response = await client.GetAsync($"/api/sections/resume/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadFromJsonAsync<ResumeService.DTOs.ApiResponse<object[]>>();
                    if (content != null && content.Success && content.Data != null)
                    {
                        sections = content.Data;
                    }
                }
                else
                {
                    _logger.LogWarning($"SectionService returned {response.StatusCode} for resume {id}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to fetch sections for resume {id} from SectionService.");
            }

            return new
            {
                resume = resume,
                sections = sections
            };
        }

        public async Task<List<Resume>> GetResumesByUser(int userId)
        {
            return await _repository.FindByUserId(userId);
        }

        public async Task<Resume?> DuplicateResume(int id, string? token = null)
        {
            try
            {
                var original = await _repository.FindByResumeId(id);
                if (original == null) return null;

                // 1. Create and Save New Resume
                var copy = new Resume
                {
                    UserId = original.UserId,
                    Title = original.Title + " (Copy)",
                    TargetJobTitle = original.TargetJobTitle,
                    TemplateId = original.TemplateId,
                    Language = original.Language,
                    Status = "DRAFT",
                    AtsScore = 0,
                    IsPublic = false,
                    ViewCount = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _repository.Add(copy);
                await _repository.SaveChanges();

                // 2. Fetch Original Sections
                List<SectionDto> sections = new();
                try
                {
                    var client = _httpClientFactory.CreateClient("SectionService");
                    if (!string.IsNullOrEmpty(token))
                    {
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));
                    }

                    var response = await client.GetAsync($"/api/sections/resume/{id}");
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadFromJsonAsync<ApiResponse<List<SectionDto>>>();
                        if (content?.Success == true && content.Data != null)
                        {
                            sections = content.Data;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch original sections for duplication of resume {Id}", id);
                }

                // 3. Copy Sections to New Resume
                if (sections.Any())
                {
                    var client = _httpClientFactory.CreateClient("SectionService");
                    if (!string.IsNullOrEmpty(token))
                    {
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Replace("Bearer ", ""));
                    }

                    foreach (var section in sections)
                    {
                        var newSection = new
                        {
                            ResumeId = copy.ResumeId,
                            SectionType = section.SectionType,
                            Title = section.Title,
                            Content = section.Content,
                            DisplayOrder = section.DisplayOrder,
                            IsVisible = section.IsVisible,
                            AiGenerated = section.AiGenerated
                        };

                        await client.PostAsJsonAsync("/api/sections", newSection);
                    }
                }

                return copy;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error duplicating resume {Id}", id);
                throw;
            }
        }

        public async Task<bool> UpdateAtsScore(int id, int score)
        {
            try
            {
                await _repository.UpdateAtsScoreAsync(id, score);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating ATS score for {Id}", id);
                throw;
            }
        }

        public async Task<bool> PublishResume(int id)
        {
            try
            {
                var resume = await _repository.FindByResumeId(id);
                if (resume == null) return false;

                resume.IsPublic = true;
                resume.UpdatedAt = DateTime.UtcNow;
                await _repository.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing resume {Id}", id);
                throw;
            }
        }

        public async Task<bool> UnpublishResume(int id)
        {
            try
            {
                var resume = await _repository.FindByResumeId(id);
                if (resume == null) return false;

                resume.IsPublic = false;
                resume.UpdatedAt = DateTime.UtcNow;
                await _repository.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unpublishing resume {Id}", id);
                throw;
            }
        }

        public async Task<List<Resume>> GetPublicResumes()
        {
            return await _repository.FindByIsPublic(true);
        }

        public async Task<bool> IncrementViewCount(int id)
        {
            try
            {
                await _repository.IncrementViewCountAsync(id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing view count for {Id}", id);
                throw;
            }
        }

        public async Task<List<Resume>> GetResumesByTemplate(int templateId)
        {
            return await _repository.FindByTemplateId(templateId);
        }

        public async Task<bool> DeleteResumeAsync(int id, int userId, string role)
        {
            var resume = await _repository.FindByResumeId(id);
            if (resume == null) return false;

            if (resume.UserId != userId && role != "Admin")
            {
                throw new UnauthorizedAccessException("You do not have permission to delete this resume.");
            }

            await _repository.DeleteByResumeId(id);
            await _repository.SaveChanges();
            return true;
        }

        public async Task<ResumeDto?> UpdateResumeAsync(int id, UpdateResumeDto dto, int userId)
        {
            var resume = await _repository.FindByResumeId(id);
            if (resume == null) return null;

            if (resume.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this resume.");
            }

            if (!string.IsNullOrEmpty(dto.Title))
                resume.Title = dto.Title;
            
            if (dto.TemplateId.HasValue)
                resume.TemplateId = dto.TemplateId.Value;

            resume.UpdatedAt = DateTime.UtcNow;

            await _repository.Update(resume);
            await _repository.SaveChanges();

            return new ResumeDto
            {
                ResumeId = resume.ResumeId,
                UserId = resume.UserId,
                Title = resume.Title,
                TargetJobTitle = resume.TargetJobTitle,
                TemplateId = resume.TemplateId,
                AtsScore = resume.AtsScore,
                Status = resume.Status,
                Language = resume.Language,
                IsPublic = resume.IsPublic,
                ViewCount = resume.ViewCount,
                CreatedAt = resume.CreatedAt,
                UpdatedAt = resume.UpdatedAt
            };
        }

        // ── Admin Analytics ───────────────────────────────────────────────────

        public async Task<ResumeAnalyticsDto> GetAdminAnalytics()
        {
            var total = await _repository.GetTotalResumeCount();
            var publicCount = await _repository.GetPublicResumeCount();
            var views = await _repository.GetTotalViewCount();
            var templateUsage = await _repository.GetTemplateUsageStats();

            return new ResumeAnalyticsDto
            {
                TotalResumes = total,
                PublicResumes = publicCount,
                TotalViews = views,
                TemplateUsage = templateUsage,
                GeneratedAt = DateTime.UtcNow
            };
        }
    }
}
