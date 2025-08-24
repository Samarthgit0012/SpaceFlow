using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SpaceFlow.Data;
using SpaceFlow.Repositories.Models;
using SpaceFlow.Services.Interfaces;
using SpaceFlow.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Web;

namespace SpaceFlow.Services.Implementation
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<AccountService> _logger;

        public AccountService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            IEmailService emailService,
            ILogger<AccountService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<(bool success, List<string> errors)> RegisterAsync(RegisterDto registerDto)
        {
            var errors = new List<string>();

            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
                if (existingUser != null)
                {
                    errors.Add("User with this email already exists.");
                    return (false, errors);
                }

                var user = new ApplicationUser
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    FullName = registerDto.FullName,
                    PhoneNumber = registerDto.PhoneNumber
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (result.Succeeded)
                {
                    // Send email confirmation
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _emailService.SendEmailConfirmationAsync(user.Email, user.FullName, code);

                    _logger.LogInformation("User {Email} registered successfully", registerDto.Email);
                    return (true, errors);
                }

                foreach (var error in result.Errors)
                {
                    errors.Add(error.Description);
                }

                return (false, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for {Email}", registerDto.Email);
                errors.Add("An error occurred during registration. Please try again.");
                return (false, errors);
            }
        }

        public async Task<(bool success, string? token, List<string> errors)> LoginAsync(LoginDto loginDto)
        {
            var errors = new List<string>();

            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    errors.Add("Invalid email or password.");
                    return (false, null, errors);
                }

                var result = await _signInManager.PasswordSignInAsync(
                    user.UserName!,
                    loginDto.Password,
                    loginDto.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // Update last login time (if you want to track this)
                    // user.LastLoginDate = DateTime.UtcNow;
                    // await _userManager.UpdateAsync(user);

                    _logger.LogInformation("User {Email} logged in successfully", loginDto.Email);

                    // For API authentication, you might want to generate a JWT token here
                    // For now, we'll return a simple success indicator
                    return (true, "success", errors);
                }

                if (result.RequiresTwoFactor)
                {
                    errors.Add("Two-factor authentication is required.");
                    return (false, null, errors);
                }

                if (result.IsLockedOut)
                {
                    errors.Add("Your account has been locked out.");
                    return (false, null, errors);
                }

                if (result.IsNotAllowed)
                {
                    errors.Add("You are not allowed to sign in. Please confirm your email.");
                    return (false, null, errors);
                }

                errors.Add("Invalid email or password.");
                return (false, null, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", loginDto.Email);
                errors.Add("An error occurred during login. Please try again.");
                return (false, null, errors);
            }
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out");
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return null;
                }

                // Get additional statistics
                var totalBookings = await _context.Bookings.CountAsync(b => b.ApplicationUserId == userId);
                var totalSpent = await _context.Payments
                    .Where(p => p.ApplicationUserId == userId)
                    .SumAsync(p => p.Amount);

                return new UserProfileDto
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FullName = user.FullName ?? string.Empty,
                    PhoneNumber = user.PhoneNumber,
                    EmailConfirmed = user.EmailConfirmed,
                    TotalBookings = totalBookings,
                    TotalSpent = totalSpent
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile for {UserId}", userId);
                return null;
            }
        }

        public async Task<(bool success, List<string> errors)> UpdateProfileAsync(string userId, UpdateProfileDto updateProfileDto)
        {
            var errors = new List<string>();

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    errors.Add("User not found.");
                    return (false, errors);
                }

                user.FullName = updateProfileDto.FullName;
                user.PhoneNumber = updateProfileDto.PhoneNumber;

                // Handle email change
                if (!string.IsNullOrEmpty(updateProfileDto.Email) && updateProfileDto.Email != user.Email)
                {
                    var emailExists = await _userManager.FindByEmailAsync(updateProfileDto.Email);
                    if (emailExists != null)
                    {
                        errors.Add("Email is already in use.");
                        return (false, errors);
                    }

                    user.Email = updateProfileDto.Email;
                    user.UserName = updateProfileDto.Email;
                    user.EmailConfirmed = false;

                    // Send new email confirmation
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    await _emailService.SendEmailConfirmationAsync(user.Email, user.FullName, code);
                }

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Profile updated for user {UserId}", userId);
                    return (true, errors);
                }

                foreach (var error in result.Errors)
                {
                    errors.Add(error.Description);
                }

                return (false, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", userId);
                errors.Add("An error occurred while updating your profile. Please try again.");
                return (false, errors);
            }
        }

        public async Task<(bool success, List<string> errors)> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
        {
            var errors = new List<string>();

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    errors.Add("User not found.");
                    return (false, errors);
                }

                var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

                if (result.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    _logger.LogInformation("Password changed for user {UserId}", userId);
                    return (true, errors);
                }

                foreach (var error in result.Errors)
                {
                    errors.Add(error.Description);
                }

                return (false, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                errors.Add("An error occurred while changing your password. Please try again.");
                return (false, errors);
            }
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return true;
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _emailService.SendPasswordResetAsync(user.Email!, user.FullName!, code);

                _logger.LogInformation("Password reset requested for {Email}", forgotPasswordDto.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing forgot password for {Email}", forgotPasswordDto.Email);
                return false;
            }
        }

        public async Task<(bool success, List<string> errors)> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var errors = new List<string>();

            try
            {
                var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist
                    errors.Add("Invalid password reset attempt.");
                    return (false, errors);
                }

                var result = await _userManager.ResetPasswordAsync(user, resetPasswordDto.Code, resetPasswordDto.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Password reset successful for {Email}", resetPasswordDto.Email);
                    return (true, errors);
                }

                foreach (var error in result.Errors)
                {
                    errors.Add(error.Description);
                }

                return (false, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for {Email}", resetPasswordDto.Email);
                errors.Add("An error occurred while resetting your password. Please try again.");
                return (false, errors);
            }
        }

        public async Task<bool> ConfirmEmailAsync(string userId, string code)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                var result = await _userManager.ConfirmEmailAsync(user, code);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Email confirmed for user {UserId}", userId);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming email for user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ResendEmailConfirmationAsync(ResendConfirmationDto resendDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(resendDto.Email);
                if (user == null || await _userManager.IsEmailConfirmedAsync(user))
                {
                    // Don't reveal that the user does not exist or email is already confirmed
                    return true;
                }

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _emailService.SendEmailConfirmationAsync(user.Email!, user.FullName!, code);

                _logger.LogInformation("Email confirmation resent for {Email}", resendDto.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resending email confirmation for {Email}", resendDto.Email);
                return false;
            }
        }

        public async Task<(bool success, List<string> errors)> DeleteAccountAsync(string userId, DeleteAccountDto deleteAccountDto)
        {
            var errors = new List<string>();

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    errors.Add("User not found.");
                    return (false, errors);
                }

                // Verify password
                var isPasswordValid = await _userManager.CheckPasswordAsync(user, deleteAccountDto.Password);
                if (!isPasswordValid)
                {
                    errors.Add("Invalid password.");
                    return (false, errors);
                }

                // Check for active bookings
                var activeBookings = await _context.Bookings
                    .Where(b => b.ApplicationUserId == userId &&
                                b.Status != BookingStatus.Canceled &&
                                b.Status != BookingStatus.Completed)
                    .CountAsync();

                if (activeBookings > 0)
                {
                    errors.Add("Cannot delete account with active bookings. Please cancel or complete all bookings first.");
                    return (false, errors);
                }

                // Soft delete or anonymize data instead of hard delete (recommended for compliance)
                user.Email = $"deleted_{Guid.NewGuid()}@deleted.com";
                user.UserName = user.Email;
                user.FullName = "Deleted User";
                user.PhoneNumber = null;
                user.EmailConfirmed = false;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    await _signInManager.SignOutAsync();
                    _logger.LogInformation("Account deleted/anonymized for user {UserId}. Reason: {Reason}", userId, deleteAccountDto.Reason);
                    return (true, errors);
                }

                foreach (var error in result.Errors)
                {
                    errors.Add(error.Description);
                }

                return (false, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account for user {UserId}", userId);
                errors.Add("An error occurred while deleting your account. Please try again.");
                return (false, errors);
            }
        }
    }
}