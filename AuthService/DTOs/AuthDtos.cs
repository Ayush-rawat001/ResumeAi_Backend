namespace AuthService.DTOs
{
    public class UserRegisterDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class UserLoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class UserInfoDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string SubscriptionPlan { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public UserInfoDto User { get; set; } = new();
    }

    public class UserProfileUpdateDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }

    public class PasswordChangeDto
    {
        public string NewPassword { get; set; } = string.Empty;
    }

    public class SubscriptionUpdateDto
    {
        public string Plan { get; set; } = string.Empty; // FREE or PREMIUM
    }

    public class AdminAnalyticsDto
    {
        public int TotalUsers { get; set; }
        public int FreeUsers { get; set; }
        public int PremiumUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int SuspendedUsers { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class UserSummaryDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string SubscriptionPlan { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
