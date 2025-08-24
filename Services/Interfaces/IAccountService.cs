using SpaceFlow.ViewModels;

namespace SpaceFlow.Services.Interfaces
{
    public interface IAccountService
    {
        Task<(bool success, List<string> errors)> RegisterAsync(RegisterDto registerDto);
        Task<(bool success, string? token, List<string> errors)> LoginAsync(LoginDto loginDto);
        Task LogoutAsync();
        Task<UserProfileDto?> GetUserProfileAsync(string userId);
        Task<(bool success, List<string> errors)> UpdateProfileAsync(string userId, UpdateProfileDto updateProfileDto);
        Task<(bool success, List<string> errors)> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<(bool success, List<string> errors)> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        Task<bool> ConfirmEmailAsync(string userId, string code);
        Task<bool> ResendEmailConfirmationAsync(ResendConfirmationDto resendDto);
        Task<(bool success, List<string> errors)> DeleteAccountAsync(string userId, DeleteAccountDto deleteAccountDto);
    }
}
