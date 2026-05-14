using AuthService.Models;
using AuthService.DTOs;

namespace AuthService.Services
{
    public interface IAuthService
    {
        Task<(string Message, int UserId)> Register(User user, string password);
        Task<(string Token, User? User)> Login(string email, string password);
        Task<User?> GetUserById(int id);
        Task<List<User>> GetAllUsers();
        Task<string> UpdateProfile(int id, string fullName, string phone);
        Task<string> ChangePassword(int id, string newPassword);
        Task<string> UpdateSubscription(int id, string plan);
        Task<string> DeactivateAccount(int id);
        // Admin-only operations
        Task<string> ReactivateAccount(int id);
        Task<string> DeleteUser(int id);
        Task<AdminAnalyticsDto> GetAnalytics();
        Task<string?> GenerateNewToken(int userId);
    }
}
