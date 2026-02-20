namespace MedicineReminder.Application.Common.Interfaces;

public record AuthData(string Token, string RefreshToken);

public interface IIdentityService
{
    Task<(AuthData? Data, string Message, string[]? Errors)> RegisterAsync(string email, string password);
    Task<(AuthData? Data, string Message, string[]? Errors)> LoginAsync(string email, string password, string? fcmToken = null);
    Task<(AuthData? Data, string Message, string[]? Errors)> RefreshTokenAsync(string accessToken, string refreshToken);
    Task<(bool Success, string Message)> VerifyEmailAsync(string userId, string token);
}
