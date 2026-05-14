namespace TemplateService.DTOs
{
    public class CreateTemplateDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string HtmlLayout { get; set; } = string.Empty;
        public string CssStyles { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsPremium { get; set; }
    }
}
