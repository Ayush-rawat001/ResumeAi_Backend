using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ResumeService.DTOs;
using ResumeService.Models;
using ResumeService.Services;
using System.Security.Claims;

namespace ResumeService.Controllers
{
    [ApiController]
    [Route("api/resumes")]
    [Authorize]
    public class ResumeController : ControllerBase
    {
        private readonly IResumeService _resumeService;

        public ResumeController(IResumeService resumeService)
        {
            _resumeService = resumeService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateResume([FromBody] Resume resume)
        {
            if (resume.UserId <= 0 || string.IsNullOrEmpty(resume.Title) || string.IsNullOrEmpty(resume.TargetJobTitle))
            {
                return BadRequest(ApiResponse<object>.Fail("UserId, Title, and TargetJobTitle are required."));
            }

            try
            {
                var created = await _resumeService.CreateResume(resume);
                return Ok(ApiResponse<Resume>.Ok(created, "Resume created successfully"));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.Fail("An unexpected error occurred while creating the resume."));
            }
        }

        [HttpPost("duplicate/{id}")]
        public async Task<IActionResult> DuplicateResume(int id)
        {
            try
            {
                var token = Request.Headers["Authorization"].ToString();
                var duplicate = await _resumeService.DuplicateResume(id, token);
                if (duplicate == null) return NotFound(ApiResponse<object>.Fail("Original resume not found."));

                return Ok(ApiResponse<Resume>.Ok(duplicate, "Resume duplicated successfully"));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.Fail("Error duplicating resume."));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetResume(int id)
        {
            var token = Request.Headers["Authorization"].ToString();
            var resume = await _resumeService.GetResumeById(id, token);
            if (resume == null) return NotFound(ApiResponse<object>.Fail("Resume not found."));

            return Ok(ApiResponse<object>.Ok(resume, "Resume fetched successfully"));
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserResumes(int userId)
        {
            var resumes = await _resumeService.GetResumesByUser(userId);
            return Ok(ApiResponse<List<Resume>>.Ok(resumes, $"Fetched {resumes.Count} resumes for user."));
        }

        [HttpGet("public")]
        public async Task<IActionResult> GetPublicResumes()
        {
            var resumes = await _resumeService.GetPublicResumes();
            return Ok(ApiResponse<List<Resume>>.Ok(resumes, "Fetched public resumes."));
        }

        [HttpGet("template/{templateId}")]
        public async Task<IActionResult> GetResumesByTemplate(int templateId)
        {
            var resumes = await _resumeService.GetResumesByTemplate(templateId);
            return Ok(ApiResponse<List<Resume>>.Ok(resumes, "Fetched resumes by template."));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateResume(int id, [FromBody] UpdateResumeDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                var updated = await _resumeService.UpdateResumeAsync(id, dto, userId);
                
                if (updated == null) return NotFound(ApiResponse<object>.Fail("Resume not found."));

                return Ok(ApiResponse<ResumeDto>.Ok(updated, "Resume updated successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.Fail("Error updating resume."));
            }
        }

        [HttpPut("publish/{id}")]
        public async Task<IActionResult> PublishResume(int id)
        {
            var success = await _resumeService.PublishResume(id);
            if (!success) return NotFound(ApiResponse<object>.Fail("Resume not found."));

            return Ok(ApiResponse<object>.Ok(null, "Resume published successfully. It is now public."));
        }

        [HttpPut("unpublish/{id}")]
        public async Task<IActionResult> UnpublishResume(int id)
        {
            var success = await _resumeService.UnpublishResume(id);
            if (!success) return NotFound(ApiResponse<object>.Fail("Resume not found."));

            return Ok(ApiResponse<object>.Ok(null, "Resume unpublished. It is now private."));
        }

        [HttpPut("ats/{id}")]
        public async Task<IActionResult> UpdateAtsScore(int id, [FromQuery] int score)
        {
            var success = await _resumeService.UpdateAtsScore(id, score);
            if (!success) return NotFound(ApiResponse<object>.Fail("Resume not found."));

            return Ok(ApiResponse<object>.Ok(null, $"ATS score updated to {score}%."));
        }

        [HttpPut("view/{id}")]
        public async Task<IActionResult> IncrementView(int id)
        {
            var success = await _resumeService.IncrementViewCount(id);
            if (!success) return NotFound(ApiResponse<object>.Fail("Resume not found."));

            return Ok(ApiResponse<object>.Ok(null, "View count incremented."));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteResume(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                var role = User.FindFirstValue(ClaimTypes.Role) ?? "User";
                
                var success = await _resumeService.DeleteResumeAsync(id, userId, role);
                if (!success) return NotFound(ApiResponse<object>.Fail("Resume not found."));

                return Ok(ApiResponse<object>.Ok(null, "Resume deleted successfully"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, ApiResponse<object>.Fail(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.Fail("Error deleting resume."));
            }
        }

        // ── Admin-only Analytics ─────────────────────────────────────────────

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/analytics")]
        public async Task<IActionResult> GetAdminAnalytics()
        {
            var analytics = await _resumeService.GetAdminAnalytics();
            return Ok(ApiResponse<ResumeAnalyticsDto>.Ok(analytics, "Resume analytics fetched successfully"));
        }
    }
}
