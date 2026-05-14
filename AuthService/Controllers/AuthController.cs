using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // ── Public endpoints ──────────────────────────────────────────────────

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            var user = new User
            {
                FullName         = dto.FullName,
                Email            = dto.Email,
                Phone            = dto.Phone,
                Role             = "USER",
                SubscriptionPlan = "FREE"
            };

            var (message, userId) = await _authService.Register(user, dto.Password);
            if (userId != 0)
                return Ok(ApiResponse<object>.Ok(new { userId }, message));

            return BadRequest(ApiResponse<object>.Fail(message));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            var (token, user) = await _authService.Login(dto.Email, dto.Password);
            if (user == null)
                return Unauthorized(ApiResponse<object>.Fail(token));

            var response = new AuthResponseDto
            {
                Token   = token,
                Message = "Login successful.",
                User    = new UserInfoDto
                {
                    UserId           = user.UserId,
                    Email            = user.Email,
                    Role             = user.Role,
                    SubscriptionPlan = user.SubscriptionPlan
                }
            };
            return Ok(ApiResponse<AuthResponseDto>.Ok(response, "Login successful."));
        }

        [HttpPost("logout")]
        public IActionResult Logout()
            => Ok(ApiResponse<object>.Ok(null, "Logged out successfully"));

        // ── Authenticated user endpoints ──────────────────────────────────────

        [Authorize]
        [HttpGet("profile/{id}")]
        public async Task<IActionResult> GetProfile(int id)
        {
            var user = await _authService.GetUserById(id);
            if (user == null) return NotFound(ApiResponse<object>.Fail("User not found."));
            return Ok(ApiResponse<User>.Ok(user, "Profile fetched successfully"));
        }

        [Authorize]
        [HttpPut("profile/{id}")]
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UserProfileUpdateDto dto)
        {
            var result = await _authService.UpdateProfile(id, dto.FullName, dto.Phone);
            if (result == "User not found.") return NotFound(ApiResponse<object>.Fail(result));
            return Ok(ApiResponse<object>.Ok(null, result));
        }

        [Authorize]
        [HttpPut("password/{id}")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] PasswordChangeDto dto)
        {
            var result = await _authService.ChangePassword(id, dto.NewPassword);
            if (result == "User not found.") return NotFound(ApiResponse<object>.Fail(result));
            return Ok(ApiResponse<object>.Ok(null, result));
        }

        // ── Admin-only: User Management ───────────────────────────────────────

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _authService.GetAllUsers();
            // Map to safe summary DTO (excludes password hash)
            var summaries = users.Select(u => new UserSummaryDto
            {
                UserId           = u.UserId,
                FullName         = u.FullName,
                Email            = u.Email,
                Phone            = u.Phone,
                Role             = u.Role,
                SubscriptionPlan = u.SubscriptionPlan,
                IsActive         = u.IsActive,
                CreatedAt        = u.CreatedAt
            }).ToList();
            return Ok(ApiResponse<List<UserSummaryDto>>.Ok(summaries, "Users fetched successfully"));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("admin/subscription/{id}")]
        public async Task<IActionResult> UpdateSubscription(int id, [FromBody] SubscriptionUpdateDto dto)
        {
            var result = await _authService.UpdateSubscription(id, dto.Plan);
            if (result == "User not found.") return NotFound(ApiResponse<object>.Fail(result));
            return Ok(ApiResponse<object>.Ok(null, result));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("admin/suspend/{id}")]
        public async Task<IActionResult> SuspendUser(int id)
        {
            var result = await _authService.DeactivateAccount(id);
            if (result == "User not found.") return NotFound(ApiResponse<object>.Fail(result));
            return Ok(ApiResponse<object>.Ok(null, result));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("admin/reactivate/{id}")]
        public async Task<IActionResult> ReactivateUser(int id)
        {
            var result = await _authService.ReactivateAccount(id);
            if (result == "User not found.") return NotFound(ApiResponse<object>.Fail(result));
            return Ok(ApiResponse<object>.Ok(null, result));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _authService.DeleteUser(id);
            if (result == "User not found.") return NotFound(ApiResponse<object>.Fail(result));
            return Ok(ApiResponse<object>.Ok(null, result));
        }

        // ── Admin-only: Analytics ─────────────────────────────────────────────

        [Authorize(Roles = "Admin")]
        [HttpGet("admin/analytics")]
        public async Task<IActionResult> GetAnalytics()
        {
            var analytics = await _authService.GetAnalytics();
            return Ok(ApiResponse<AdminAnalyticsDto>.Ok(analytics, "Analytics fetched successfully"));
        }

        // ── User self-upgrade to Premium ────────────────────────────────────

        [Authorize]
        [HttpPost("upgrade")]
        public async Task<IActionResult> UpgradeToPremium()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(ApiResponse<object>.Fail("Invalid token."));

            var result = await _authService.UpdateSubscription(userId, "PREMIUM");
            if (result == "User not found.")
                return NotFound(ApiResponse<object>.Fail(result));

            // Generate a fresh token with updated SubscriptionPlan claim
            var newToken = await _authService.GenerateNewToken(userId);
            var user = await _authService.GetUserById(userId);

            return Ok(ApiResponse<object>.Ok(new
            {
                token = newToken,
                user = new UserInfoDto
                {
                    UserId = user!.UserId,
                    Email = user.Email,
                    Role = user.Role,
                    SubscriptionPlan = user.SubscriptionPlan
                }
            }, "Upgraded to PREMIUM successfully!"));
        }

        // ── Policy-based (PremiumOnly) ────────────────────────────────────────

        [Authorize(Policy = "PremiumOnly")]
        [HttpGet("premium")]
        public IActionResult GetPremiumContent()
            => Ok(ApiResponse<object>.Ok(null, "Welcome Premium User"));
    }
}
