using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SpaceFlow.Repositories.Models;
using SpaceFlow.Services.Interfaces;
using SpaceFlow.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SpaceFlow.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(IAccountService accountService, UserManager<ApplicationUser> userManager)
        {
            _accountService = accountService;
            _userManager = userManager;
        }

        /// <summary>
        /// Register a new user account
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, errors) = await _accountService.RegisterAsync(registerDto);

            if (success)
            {
                return Ok(new { message = "Registration successful. Please check your email to confirm your account." });
            }

            return BadRequest(new { message = "Registration failed", errors });
        }

        /// <summary>
        /// Login with email and password
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, token, errors) = await _accountService.LoginAsync(loginDto);

            if (success)
            {
                return Ok(new { message = "Login successful", token });
            }

            return BadRequest(new { message = "Login failed", errors });
        }

        /// <summary>
        /// Logout current user
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutAsync();
            return Ok(new { message = "Logout successful" });
        }

        /// <summary>
        /// Get current user profile information
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var userProfile = await _accountService.GetUserProfileAsync(userId);
            if (userProfile == null)
            {
                return NotFound(new { message = "User not found" });
            }

            return Ok(userProfile);
        }

        /// <summary>
        /// Update user profile information
        /// </summary>
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var (success, errors) = await _accountService.UpdateProfileAsync(userId, updateProfileDto);

            if (success)
            {
                return Ok(new { message = "Profile updated successfully" });
            }

            return BadRequest(new { message = "Profile update failed", errors });
        }

        /// <summary>
        /// Change user password
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var (success, errors) = await _accountService.ChangePasswordAsync(userId, changePasswordDto);

            if (success)
            {
                return Ok(new { message = "Password changed successfully" });
            }

            return BadRequest(new { message = "Password change failed", errors });
        }

        /// <summary>
        /// Request password reset
        /// </summary>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _accountService.ForgotPasswordAsync(forgotPasswordDto);

            if (success)
            {
                return Ok(new { message = "Password reset link has been sent to your email." });
            }

            return BadRequest(new { message = "Failed to send password reset link" });
        }

        /// <summary>
        /// Reset password with token
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, errors) = await _accountService.ResetPasswordAsync(resetPasswordDto);

            if (success)
            {
                return Ok(new { message = "Password reset successfully" });
            }

            return BadRequest(new { message = "Password reset failed", errors });
        }

        /// <summary>
        /// Confirm email address
        /// </summary>
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string code)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
            {
                return BadRequest(new { message = "Invalid email confirmation link" });
            }

            var success = await _accountService.ConfirmEmailAsync(userId, code);

            if (success)
            {
                return Ok(new { message = "Email confirmed successfully" });
            }

            return BadRequest(new { message = "Email confirmation failed" });
        }

        /// <summary>
        /// Resend email confirmation
        /// </summary>
        [HttpPost("resend-confirmation")]
        public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendConfirmationDto resendDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _accountService.ResendEmailConfirmationAsync(resendDto);

            if (success)
            {
                return Ok(new { message = "Confirmation email sent successfully" });
            }

            return BadRequest(new { message = "Failed to send confirmation email" });
        }

        /// <summary>
        /// Delete user account
        /// </summary>
        [HttpDelete("delete-account")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount([FromBody] DeleteAccountDto deleteAccountDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var (success, errors) = await _accountService.DeleteAccountAsync(userId, deleteAccountDto);

            if (success)
            {
                return Ok(new { message = "Account deleted successfully" });
            }

            return BadRequest(new { message = "Account deletion failed", errors });
        }

        /// <summary>
        /// Check if user is authenticated
        /// </summary>
        [HttpGet("status")]
        [Authorize]
        public IActionResult GetAuthStatus()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var fullName = User.FindFirstValue("FullName");

            return Ok(new
            {
                isAuthenticated = true,
                userId,
                email,
                fullName
            });
        }
    }
}