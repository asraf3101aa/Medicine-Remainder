using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Application.Common.Models;
using MedicineReminder.Domain.Entities;

namespace MedicineReminder.Application.Common.Interfaces;

public interface IUserDeviceService : IEntityService<UserDevice>
{
    Task<ServiceResult<IEnumerable<UserDevice>>> GetUserDevicesAsync(string userId);
    Task<ServiceResult<PaginatedList<UserDevice>>> GetUserDevicesPaginatedAsync(string userId, PaginationQuery query);
    Task<ServiceResult<UserDevice>> GetDeviceByFcmTokenAsync(string fcmToken);
    Task<ServiceResult<bool>> IsDeviceRegisteredAsync(string userId, string deviceName);
    Task<ServiceResult<UserDevice>> RegisterDeviceAsync(string userId, string fcmToken, string deviceName);
    Task<ServiceResult> UpdateFcmTokenAsync(string deviceId, string newFcmToken);
    Task<ServiceResult<int>> GetUserDevicesCountAsync(string userId);
    Task<ServiceResult<IEnumerable<string>>> GetUserFcmTokensAsync(string userId);
}