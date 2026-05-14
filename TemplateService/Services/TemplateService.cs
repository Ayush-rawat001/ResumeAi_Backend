using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TemplateService.DTOs;
using TemplateService.Models;
using TemplateService.Repositories;

namespace TemplateService.Services
{
    public class TemplateService : ITemplateService
    {
        private readonly ITemplateRepository _repository;

        public TemplateService(ITemplateRepository repository)
        {
            _repository = repository;
        }

        public async Task<object> CreateTemplate(CreateTemplateDto dto)
        {
            try
            {
                var template = new ResumeTemplate
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    ThumbnailUrl = dto.ThumbnailUrl,
                    HtmlLayout = dto.HtmlLayout,
                    CssStyles = dto.CssStyles,
                    Category = dto.Category,
                    IsPremium = dto.IsPremium,
                    IsActive = true,
                    UsageCount = 0,
                    CreatedAt = DateTime.UtcNow
                };

                await _repository.AddAsync(template);
                await _repository.SaveChangesAsync();

                return ApiResponse<TemplateResponseDto>.Ok(MapToDto(template), "Template created successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Fail($"Error creating template: {ex.Message}");
            }
        }

        public async Task<object> GetAllTemplates()
        {
            var templates = await _repository.GetAllAsync();
            var activeTemplates = templates.Where(t => t.IsActive).Select(MapToDto).ToList();
            return ApiResponse<List<TemplateResponseDto>>.Ok(activeTemplates, "Active templates retrieved successfully");
        }

        public async Task<object> GetAllTemplatesAdmin()
        {
            var templates = await _repository.GetAllAsync();
            var allTemplates = templates.Select(MapToDto).ToList();
            return ApiResponse<List<TemplateResponseDto>>.Ok(allTemplates, "All templates retrieved successfully for admin");
        }

        public async Task<object> GetTemplateById(int id)
        {
            var template = await _repository.GetByIdAsync(id);
            if (template == null || !template.IsActive)
            {
                return ApiResponse<object>.Fail("Template not found or inactive");
            }

            return ApiResponse<TemplateResponseDto>.Ok(MapToDto(template), "Template retrieved successfully");
        }

        public async Task<object> GetByCategory(string category)
        {
            var templates = await _repository.GetByCategoryAsync(category);
            var activeTemplates = templates.Where(t => t.IsActive).Select(MapToDto).ToList();
            return ApiResponse<List<TemplateResponseDto>>.Ok(activeTemplates, $"Templates in category {category} retrieved");
        }

        public async Task<object> GetFreeTemplates()
        {
            var templates = await _repository.GetFreeTemplatesAsync();
            var activeTemplates = templates.Where(t => t.IsActive).Select(MapToDto).ToList();
            return ApiResponse<List<TemplateResponseDto>>.Ok(activeTemplates, "Free templates retrieved successfully");
        }

        public async Task<object> GetPremiumTemplates()
        {
            var templates = await _repository.GetPremiumTemplatesAsync();
            var activeTemplates = templates.Where(t => t.IsActive).Select(MapToDto).ToList();
            return ApiResponse<List<TemplateResponseDto>>.Ok(activeTemplates, "Premium templates retrieved successfully");
        }

        public async Task<object> GetPopularTemplates()
        {
            var templates = await _repository.GetPopularTemplatesAsync();
            var activeTemplates = templates.Where(t => t.IsActive).Select(MapToDto).ToList();
            return ApiResponse<List<TemplateResponseDto>>.Ok(activeTemplates, "Popular templates retrieved successfully");
        }

        public async Task<object> UpdateTemplate(int id, UpdateTemplateDto dto)
        {
            try
            {
                var template = await _repository.GetByIdAsync(id);
                if (template == null)
                {
                    return ApiResponse<object>.Fail("Template not found");
                }

                template.Name = dto.Name;
                template.Description = dto.Description;
                template.ThumbnailUrl = dto.ThumbnailUrl;
                template.HtmlLayout = dto.HtmlLayout;
                template.CssStyles = dto.CssStyles;
                template.Category = dto.Category;
                template.IsPremium = dto.IsPremium;
                template.IsActive = dto.IsActive;

                await _repository.UpdateAsync(template);
                await _repository.SaveChangesAsync();

                return ApiResponse<TemplateResponseDto>.Ok(MapToDto(template), "Template updated successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Fail($"Error updating template: {ex.Message}");
            }
        }

        public async Task<object> DeactivateTemplate(int id)
        {
            try
            {
                var template = await _repository.GetByIdAsync(id);
                if (template == null)
                {
                    return ApiResponse<object>.Fail("Template not found");
                }

                // Toggle logic
                template.IsActive = !template.IsActive;
                
                await _repository.UpdateAsync(template);
                await _repository.SaveChangesAsync();

                string status = template.IsActive ? "activated" : "deactivated";
                return ApiResponse<object>.Ok(null, $"Template {status} successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Fail($"Error toggling template status: {ex.Message}");
            }
        }

        public async Task<object> IncrementUsage(int id)
        {
            try
            {
                await _repository.IncrementUsageCountAsync(id);
                return ApiResponse<object>.Ok(null, "Template usage incremented successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<object>.Fail($"Error incrementing template usage: {ex.Message}");
            }
        }

        private TemplateResponseDto MapToDto(ResumeTemplate template)
        {
            return new TemplateResponseDto
            {
                TemplateId = template.TemplateId,
                Name = template.Name,
                Description = template.Description,
                ThumbnailUrl = template.ThumbnailUrl,
                Category = template.Category,
                IsPremium = template.IsPremium,
                IsActive = template.IsActive,
                UsageCount = template.UsageCount,
                CreatedAt = template.CreatedAt,
                HtmlLayout = template.HtmlLayout,
                CssStyles = template.CssStyles
            };
        }
    }
}
