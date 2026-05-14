using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AIService.DTOs;
using AIService.Services;
using System.Security.Claims;

namespace AIService.Controllers
{
    [ApiController]
    [Route("api/ai")]
    [Authorize]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;

        public AiController(IAiService aiService)
        {
            _aiService = aiService;
        }

        // ── User AI Endpoints ─────────────────────────────────────────────────

        [HttpPost("generate-summary")]
        public async Task<IActionResult> GenerateSummary([FromBody] AiRequestDto dto)
        {
            var result = await _aiService.GenerateSummary(GetUserId(), dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("generate-bullets")]
        public async Task<IActionResult> GenerateBullets([FromBody] AiRequestDto dto)
        {
            var result = await _aiService.GenerateBulletPoints(GetUserId(), dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("improve-section")]
        public async Task<IActionResult> ImproveSection([FromBody] AiRequestDto dto)
        {
            var result = await _aiService.ImproveSection(GetUserId(), dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("check-ats")]
        public async Task<IActionResult> CheckAts([FromBody] AiRequestDto dto)
        {
            var result = await _aiService.CheckAts(GetUserId(), dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPost("suggest-skills")]
        public async Task<IActionResult> SuggestSkills([FromBody] AiRequestDto dto)
        {
            var result = await _aiService.SuggestSkills(GetUserId(), dto);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var result = await _aiService.GetHistory(GetUserId());
            return Ok(result);
        }

        [HttpGet("quota")]
        public async Task<IActionResult> GetQuota()
        {
            var result = await _aiService.GetRemainingQuota(GetUserId());
            return Ok(result);
        }

        // ── Admin-only: AI Usage Statistics ──────────────────────────────────

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/stats")]
        public async Task<IActionResult> GetAdminStats()
        {
            var result = await _aiService.GetAdminStats();
            return result.Success ? Ok(result) : StatusCode(500, result);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("id")?.Value;
            return int.TryParse(claim, out int id) ? id : 0;
        }
    }
}
