using System.ComponentModel.DataAnnotations;

namespace AIService.Entities
{
    public class AiRequest
    {
        [Key]
        public Guid RequestId { get; set; } = Guid.NewGuid();

        [Required]
        public int UserId { get; set; }

        [Required]
        public int ResumeId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RequestType { get; set; } = string.Empty;

        [Required]
        public string InputPrompt { get; set; } = string.Empty;

        public string AiResponse { get; set; } = string.Empty;

        [MaxLength(20)]
        public string Model { get; set; } = "GROQ";

        public int TokensUsed { get; set; } = 0;

        [MaxLength(20)]
        public string Status { get; set; } = "QUEUED"; // QUEUED, COMPLETED, FAILED

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }
    }
}
