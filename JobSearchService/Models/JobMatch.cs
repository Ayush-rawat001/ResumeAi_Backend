using System;
using System.ComponentModel.DataAnnotations;

namespace JobSearchService.Models
{
    public class JobMatch
    {
        [Key]
        public int MatchId { get; set; }
        public int ResumeId { get; set; }
        public int UserId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string JobUrl { get; set; } = string.Empty;
        public int MatchScore { get; set; } // 0-100
        public string? MissingSkills { get; set; } // Comma-separated
        public string Source { get; set; } = "HIMALAYAS";
        public DateTime MatchedAt { get; set; } = DateTime.UtcNow;
        public bool IsBookmarked { get; set; }
    }
}
