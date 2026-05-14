using System.ComponentModel.DataAnnotations;

namespace AuthService.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "USER"; // USER or ADMIN

        [Required]
        public string Provider { get; set; } = "LOCAL"; // LOCAL, GOOGLE, LINKEDIN

        [Required]
        public string SubscriptionPlan { get; set; } = "FREE"; // FREE or PREMIUM

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
