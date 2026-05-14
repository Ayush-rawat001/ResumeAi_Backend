using SectionService.Models;

namespace SectionService.DTOs
{
    public class SectionDto
    {
        public int SectionId { get; set; }
        public int ResumeId { get; set; }
        public SectionType SectionType { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsVisible { get; set; }
        public bool AiGenerated { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UpdateSectionDto
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int? DisplayOrder { get; set; }
    }
}
