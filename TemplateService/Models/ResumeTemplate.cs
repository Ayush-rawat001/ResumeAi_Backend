using System;
using System.ComponentModel.DataAnnotations;

namespace TemplateService.Models
{
    public class ResumeTemplate
    {
        [Key]
        public int TemplateId { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public string ThumbnailUrl { get; set; } = string.Empty;
        
        public string HtmlLayout { get; set; } = string.Empty;
        
        public string CssStyles { get; set; } = string.Empty;
        
        public string Category { get; set; } = string.Empty;
        
        public bool IsPremium { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public int UsageCount { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
