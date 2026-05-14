using JobSearchService.DTOs;
using JobSearchService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace JobSearchService.Controllers
{
    [ApiController]
    [Route("api/job-search")]
    [Authorize]
    public class JobSearchController : ControllerBase
    {
        private readonly IJobSearchService _jobService;

        public JobSearchController(IJobSearchService jobService)
        {
            _jobService = jobService;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeJobFit([FromBody] AnalyzeJobFitRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _jobService.SearchAndMatchJobs(userId, request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _jobService.GetSavedMatches(userId);
            return Ok(result);
        }

        [HttpPost("bookmark/{matchId}")]
        public async Task<IActionResult> Bookmark(int matchId)
        {
            var result = await _jobService.BookmarkMatch(matchId);
            return result.Success ? Ok(result) : NotFound(result);
        }
    }
}
