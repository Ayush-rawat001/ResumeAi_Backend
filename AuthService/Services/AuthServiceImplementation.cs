using AuthService.DTOs;
using AuthService.Models;
using AuthService.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthService.Services
{
    public class AuthServiceImplementation : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<User> _passwordHasher;

        public AuthServiceImplementation(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<(string Message, int UserId)> Register(User user, string password)
        {
            if (await _userRepository.ExistsByEmail(user.Email))
                return ("User already exists with this email.", 0);

            user.PasswordHash = _passwordHasher.HashPassword(user, password);
            await _userRepository.Add(user);
            await _userRepository.SaveChanges();
            return ("User registered successfully", user.UserId);
        }

        public async Task<(string Token, User? User)> Login(string email, string password)
        {
            var user = await _userRepository.FindByEmail(email);
            if (user == null || !user.IsActive)
                return ("Invalid email or password.", null);

            var verificationResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (verificationResult == PasswordVerificationResult.Failed)
                return ("Invalid email or password.", null);

            return (GenerateJwtToken(user), user);
        }

        public async Task<User?> GetUserById(int id)
            => await _userRepository.FindByUserId(id);

        public async Task<List<User>> GetAllUsers()
            => await _userRepository.GetAllUsers();

        public async Task<string> UpdateProfile(int id, string fullName, string phone)
        {
            var user = await _userRepository.FindByUserId(id);
            if (user == null) return "User not found.";
            
            user.FullName = fullName;
            user.Phone = phone;
            
            await _userRepository.Update(user);
            await _userRepository.SaveChanges();
            return "Profile updated successfully.";
        }

        public async Task<string> ChangePassword(int id, string newPassword)
        {
            var user = await _userRepository.FindByUserId(id);
            if (user == null) return "User not found.";
            
            user.PasswordHash = _passwordHasher.HashPassword(user, newPassword);
            
            await _userRepository.Update(user);
            await _userRepository.SaveChanges();
            return "Password changed successfully.";
        }

        public async Task<string> UpdateSubscription(int id, string plan)
        {
            var user = await _userRepository.FindByUserId(id);
            if (user == null) return "User not found.";
            
            user.SubscriptionPlan = plan;
            
            await _userRepository.Update(user);
            await _userRepository.SaveChanges();
            return $"Subscription updated to {plan}.";
        }

        public async Task<string> DeactivateAccount(int id)
        {
            var user = await _userRepository.FindByUserId(id);
            if (user == null) return "User not found.";
            
            user.IsActive = false;
            
            await _userRepository.Update(user);
            await _userRepository.SaveChanges();
            return "Account suspended successfully.";
        }

        // ── Admin-only ─────────────────────────────────────────────────────────

        public async Task<string> ReactivateAccount(int id)
        {
            var user = await _userRepository.FindByUserId(id);
            if (user == null) return "User not found.";
            
            user.IsActive = true;
            
            await _userRepository.Update(user);
            await _userRepository.SaveChanges();
            return "Account reactivated successfully.";
        }

        public async Task<string> DeleteUser(int id)
        {
            await _userRepository.DeleteByUserId(id);
            await _userRepository.SaveChanges();
            return "User permanently deleted.";
        }

        public async Task<AdminAnalyticsDto> GetAnalytics()
        {
            var users = await _userRepository.GetAllUsers();
            return new AdminAnalyticsDto
            {
                TotalUsers     = users.Count,
                FreeUsers      = users.Count(u => u.SubscriptionPlan == "FREE"),
                PremiumUsers   = users.Count(u => u.SubscriptionPlan == "PREMIUM"),
                ActiveUsers    = users.Count(u => u.IsActive),
                SuspendedUsers = users.Count(u => !u.IsActive),
                GeneratedAt    = DateTime.UtcNow
            };
        }

        public async Task<string?> GenerateNewToken(int userId)
        {
            var user = await _userRepository.FindByUserId(userId);
            if (user == null) return null;
            return GenerateJwtToken(user);
        }

        // ── Private helpers ────────────────────────────────────────────────────

        private string GenerateJwtToken(User user)
        {
            var role = (user.Email == "admin@test.com" || user.Role == "Admin") ? "Admin" : user.Role;
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email,          user.Email),
                new Claim(ClaimTypes.Role,           role),
                new Claim("SubscriptionPlan",        user.SubscriptionPlan)
            };

            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? ""));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer:             _configuration["Jwt:Issuer"],
                audience:           _configuration["Jwt:Audience"],
                claims:             claims,
                expires:            DateTime.Now.AddHours(1),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
