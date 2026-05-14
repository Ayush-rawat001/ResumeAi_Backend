namespace ResumeService.DTOs
{
    public class ResumeDto
    {
        public int ResumeId { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string TargetJobTitle { get; set; } = string.Empty;
        public int TemplateId { get; set; }
        public int AtsScore { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UpdateResumeDto
    {
        public string? Title { get; set; }
        public int? TemplateId { get; set; }
    }

    public class ResumeAnalyticsDto
    {
        public int TotalResumes { get; set; }
        public int PublicResumes { get; set; }
        public int TotalViews { get; set; }
        public Dictionary<int, int> TemplateUsage { get; set; } = new();
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class SectionDto
    {
        public int SectionId { get; set; }
        public int ResumeId { get; set; }
        public string SectionType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsVisible { get; set; }
        public bool AiGenerated { get; set; }
    }
}
