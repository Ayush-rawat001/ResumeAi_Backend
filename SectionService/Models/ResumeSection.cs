using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace SectionService.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SectionType
    {
        [EnumMember(Value = "SUMMARY")] SUMMARY,
        [EnumMember(Value = "EXPERIENCE")] EXPERIENCE,
        [EnumMember(Value = "EDUCATION")] EDUCATION,
        [EnumMember(Value = "SKILLS")] SKILLS,
        [EnumMember(Value = "CERTIFICATIONS")] CERTIFICATIONS,
        [EnumMember(Value = "PROJECTS")] PROJECTS,
        [EnumMember(Value = "LANGUAGES")] LANGUAGES,
        [EnumMember(Value = "VOLUNTEER")] VOLUNTEER,
        [EnumMember(Value = "CUSTOM")] CUSTOM
    }

    public class ResumeSection
    {
        [Key]
        public int SectionId { get; set; }

        [Required]
        public int ResumeId { get; set; }

        [Required]
        public SectionType SectionType { get; set; } = SectionType.CUSTOM;

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty; // Store JSON or rich text

        public int DisplayOrder { get; set; }

        public bool IsVisible { get; set; } = true;

        public bool AiGenerated { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
