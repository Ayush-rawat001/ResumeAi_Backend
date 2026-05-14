using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SectionService.DTOs;
using SectionService.Models;
using SectionService.Services;

namespace SectionService.Controllers
{
    [ApiController]
    [Route("api/sections")]
    [Authorize]
    public class SectionController : ControllerBase
    {
        private readonly ISectionService _sectionService;

        public SectionController(ISectionService sectionService)
        {
            _sectionService = sectionService;
        }

        [HttpPost]
        public async Task<IActionResult> AddSection([FromBody] ResumeSection section)
        {
            if (section.ResumeId <= 0)
            {
                return BadRequest(ApiResponse<object>.Fail("ResumeId is required."));
            }

            try
            {
                var created = await _sectionService.AddSection(section);
                return Ok(ApiResponse<ResumeSection>.Ok(created, "Section added successfully"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.Fail("An unexpected error occurred."));
            }
        }

        [HttpGet("resume/{resumeId}")]
        public async Task<IActionResult> GetResumeSections(int resumeId)
        {
            var sections = await _sectionService.GetSectionsByResume(resumeId);
            return Ok(ApiResponse<List<ResumeSection>>.Ok(sections, $"Fetched {sections.Count} sections."));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSection(int id)
        {
            var section = await _sectionService.GetSectionById(id);
            if (section == null) return NotFound(ApiResponse<object>.Fail("Section not found."));

            return Ok(ApiResponse<ResumeSection>.Ok(section));
        }

        [HttpGet("type/{resumeId}/{type}")]
        public async Task<IActionResult> GetSectionsByType(int resumeId, string type)
        {
            var sections = await _sectionService.GetSectionsByType(resumeId, type);
            return Ok(ApiResponse<List<ResumeSection>>.Ok(sections, $"Fetched {sections.Count} sections of type {type}."));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSection(int id, [FromBody] UpdateSectionDto dto)
        {
            try
            {
                var updated = await _sectionService.UpdateSectionAsync(id, dto);
                if (updated == null) return NotFound(ApiResponse<object>.Fail("Section not found."));

                return Ok(ApiResponse<SectionDto>.Ok(updated, "Section updated successfully"));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.Fail("Error updating section."));
            }
        }

        [HttpPut("reorder/{resumeId}")]
        public async Task<IActionResult> ReorderSections(int resumeId, [FromBody] List<int> orderedSectionIds)
        {
            // reorder logic: Updates DisplayOrder sequentially for the provided IDs.
            var success = await _sectionService.ReorderSections(resumeId, orderedSectionIds);
            if (!success) return BadRequest(ApiResponse<object>.Fail("Failed to reorder. Ensure all IDs belong to this resume."));

            return Ok(ApiResponse<object>.Ok(null, "Sections reordered successfully."));
        }

        [HttpPut("toggle/{id}")]
        public async Task<IActionResult> ToggleVisibility(int id)
        {
            // visibility toggle: Flips the IsVisible status.
            var success = await _sectionService.ToggleVisibility(id);
            if (!success) return NotFound(ApiResponse<object>.Fail("Section not found."));

            return Ok(ApiResponse<object>.Ok(null, "Visibility toggled successfully."));
        }

        [HttpPut("bulk/{resumeId}")]
        public async Task<IActionResult> BulkUpdate(int resumeId, [FromBody] List<ResumeSection> sections)
        {
            // bulk update: Updates multiple sections in a single request.
            var success = await _sectionService.BulkUpdateSections(resumeId, sections);
            if (!success) return BadRequest(ApiResponse<object>.Fail("Bulk update failed."));

            return Ok(ApiResponse<object>.Ok(null, "Sections updated in bulk successfully."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSection(int id)
        {
            var success = await _sectionService.DeleteSectionAsync(id);
            if (!success) return NotFound(ApiResponse<object>.Fail("Section not found."));

            return Ok(ApiResponse<object>.Ok(null, "Section deleted successfully."));
        }

        [HttpDelete("resume/{resumeId}")]
        public async Task<IActionResult> DeleteAllResumeSections(int resumeId)
        {
            var success = await _sectionService.DeleteAllSections(resumeId);
            if (!success) return BadRequest(ApiResponse<object>.Fail("Delete failed."));

            return Ok(ApiResponse<object>.Ok(null, "All sections for the resume deleted."));
        }
    }
}
