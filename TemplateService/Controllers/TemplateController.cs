using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TemplateService.DTOs;
using TemplateService.Services;

namespace TemplateService.Controllers
{
    [ApiController]
    [Route("api/templates")]
    public class TemplateController : ControllerBase
    {
        private readonly ITemplateService _templateService;

        public TemplateController(ITemplateService templateService)
        {
            _templateService = templateService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTemplate([FromBody] CreateTemplateDto dto)
        {
            var result = await _templateService.CreateTemplate(dto);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTemplates()
        {
            var result = await _templateService.GetAllTemplates();
            return Ok(result);
        }

        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllTemplatesAdmin()
        {
            var result = await _templateService.GetAllTemplatesAdmin();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTemplateById(int id)
        {
            var result = await _templateService.GetTemplateById(id);
            return Ok(result);
        }

        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetByCategory(string category)
        {
            var result = await _templateService.GetByCategory(category);
            return Ok(result);
        }

        [HttpGet("free")]
        public async Task<IActionResult> GetFreeTemplates()
        {
            var result = await _templateService.GetFreeTemplates();
            return Ok(result);
        }

        [HttpGet("premium")]
        public async Task<IActionResult> GetPremiumTemplates()
        {
            var result = await _templateService.GetPremiumTemplates();
            return Ok(result);
        }

        [HttpGet("popular")]
        public async Task<IActionResult> GetPopularTemplates()
        {
            var result = await _templateService.GetPopularTemplates();
            return Ok(result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTemplate(int id, [FromBody] UpdateTemplateDto dto)
        {
            var result = await _templateService.UpdateTemplate(id, dto);
            return Ok(result);
        }

        [HttpPut("deactivate/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateTemplate(int id)
        {
            var result = await _templateService.DeactivateTemplate(id);
            return Ok(result);
        }

        [HttpPut("use/{id}")]
        public async Task<IActionResult> IncrementUsage(int id)
        {
            var result = await _templateService.IncrementUsage(id);
            return Ok(result);
        }
    }
}
