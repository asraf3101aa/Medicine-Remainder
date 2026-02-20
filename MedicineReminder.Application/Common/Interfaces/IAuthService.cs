using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Application.Common.Models;
using MedicineReminder.Domain.Entities;

namespace MedicineReminder.Application.Common.Interfaces;

public interface IAuthService<T>
{
    Task<ServiceResult<AuthTokens>> LoginAsync(string email, string password, string? fcmToken = null, string? deviceName = null);
    Task<ServiceResult<AuthTokens>> RefreshTokensAsync(string refreshToken);
    Task<ServiceResult<T>> RegisterAsync(T entity);
    Task<ServiceResult<bool>> VerifyEmailAsync(string userId, string token);
}