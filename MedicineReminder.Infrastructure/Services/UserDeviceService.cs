using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Application.Common.Models;
using MedicineReminder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicineReminder.Infrastructure.Services;

public class UserDeviceService : IUserDeviceService
{
    private readonly IMedicineReminderDbContext _context;

    public UserDeviceService(IMedicineReminderDbContext context)
    {
        _context = context;
    }

    // IEntityService implementations
    public async Task<ServiceResult<UserDevice>> GetByIdAsync(string id)
    {
        var device = await _context.UserDevices
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

        return device != null ? ServiceResult<UserDevice>.Success(device) : ServiceResult<UserDevice>.Failure("Device not found");
    }

    public async Task<ServiceResult<PaginatedList<UserDevice>>> GetAllAsync(PaginationQuery query)
    {
        var dbQuery = _context.UserDevices
            .Include(d => d.User)
            .Where(d => !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAtUtc);

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return ServiceResult<PaginatedList<UserDevice>>.Success(new PaginatedList<UserDevice>(items, totalCount, query.PageNumber, query.PageSize));
    }

    public async Task<ServiceResult<UserDevice>> CreateAsync(UserDevice entity)
    {
        await _context.UserDevices.AddAsync(entity);
        await _context.SaveChangesAsync(CancellationToken.None);
        return ServiceResult<UserDevice>.Success(entity);
    }

    public async Task<ServiceResult> UpdateAsync(UserDevice entity)
    {
        _context.UserDevices.Update(entity);
        await _context.SaveChangesAsync(CancellationToken.None);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(string id)
    {
        var result = await GetByIdAsync(id);
        if (result.IsSuccess && result.Data != null)
        {
            result.Data.IsDeleted = true;
            result.Data.DeletedAt = DateTime.UtcNow;
            await UpdateAsync(result.Data);
            return ServiceResult.Success();
        }
        return ServiceResult.Failure("Device not found");
    }



    // UserDevice specific methods
    public async Task<ServiceResult<IEnumerable<UserDevice>>> GetUserDevicesAsync(string userId)
    {
        var devices = await _context.UserDevices
            .Where(d => d.UserId == userId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToListAsync();

        return ServiceResult<IEnumerable<UserDevice>>.Success(devices);
    }

    public async Task<ServiceResult<PaginatedList<UserDevice>>> GetUserDevicesPaginatedAsync(string userId, PaginationQuery query)
    {
        var dbQuery = _context.UserDevices
            .Where(d => d.UserId == userId && !d.IsDeleted)
            .OrderByDescending(d => d.CreatedAtUtc);

        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return ServiceResult<PaginatedList<UserDevice>>.Success(new PaginatedList<UserDevice>(items, totalCount, query.PageNumber, query.PageSize));
    }

    public async Task<ServiceResult<UserDevice>> GetDeviceByFcmTokenAsync(string fcmToken)
    {
        var device = await _context.UserDevices
            .FirstOrDefaultAsync(d => d.FcmToken == fcmToken && !d.IsDeleted);

        return device != null ? ServiceResult<UserDevice>.Success(device) : ServiceResult<UserDevice>.Failure("Device not found");
    }

    public async Task<ServiceResult<bool>> IsDeviceRegisteredAsync(string userId, string deviceName)
    {
        var registered = await _context.UserDevices
            .AnyAsync(d => d.UserId == userId &&
                          d.DeviceName == deviceName &&
                          !d.IsDeleted);
        return ServiceResult<bool>.Success(registered);
    }

    public async Task<ServiceResult<UserDevice>> RegisterDeviceAsync(string userId, string fcmToken, string deviceName)
    {
        var existingDevice = await _context.UserDevices
            .FirstOrDefaultAsync(d => d.UserId == userId &&
                                     d.DeviceName == deviceName &&
                                     !d.IsDeleted);

        if (existingDevice != null)
        {
            existingDevice.FcmToken = fcmToken;
            existingDevice.UpdatedAt = DateTime.UtcNow;
            await UpdateAsync(existingDevice);
            return ServiceResult<UserDevice>.Success(existingDevice);
        }

        var device = new UserDevice
        {
            UserId = userId,
            FcmToken = fcmToken,
            DeviceName = deviceName
        };

        return await CreateAsync(device);
    }

    public async Task<ServiceResult> UpdateFcmTokenAsync(string deviceId, string newFcmToken)
    {
        var result = await GetByIdAsync(deviceId);
        if (result.IsSuccess && result.Data != null)
        {
            result.Data.FcmToken = newFcmToken;
            await UpdateAsync(result.Data);
            return ServiceResult.Success();
        }
        return ServiceResult.Failure("Device not found");
    }

    public async Task<ServiceResult<int>> GetUserDevicesCountAsync(string userId)
    {
        var count = await _context.UserDevices
            .CountAsync(d => d.UserId == userId && !d.IsDeleted);
        return ServiceResult<int>.Success(count);
    }

    public async Task<ServiceResult<IEnumerable<string>>> GetUserFcmTokensAsync(string userId)
    {
        var tokens = await _context.UserDevices
            .Where(d => d.UserId == userId &&
                       !d.IsDeleted &&
                       d.FcmToken != null)
            .Select(d => d.FcmToken!)
            .ToListAsync();

        return ServiceResult<IEnumerable<string>>.Success(tokens);
    }
}