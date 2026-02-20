using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Application.Common.Models;
using MedicineReminder.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MedicineReminder.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly IMedicineReminderDbContext _context;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UserService(
        UserManager<User> userManager,
        IMedicineReminderDbContext context,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _context = context;
        _roleManager = roleManager;
    }

    public async Task<ServiceResult<User>> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        return user != null ? ServiceResult<User>.Success(user) : ServiceResult<User>.Failure("User not found");
    }

    public async Task<ServiceResult<PaginatedList<User>>> GetAllAsync(PaginationQuery query)
    {
        var dbQuery = _userManager.Users;
        var totalCount = await dbQuery.CountAsync();
        var items = await dbQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return ServiceResult<PaginatedList<User>>.Success(new PaginatedList<User>(items, totalCount, query.PageNumber, query.PageSize));
    }

    public async Task<ServiceResult<User>> CreateAsync(User entity)
    {
        var result = await _userManager.CreateAsync(entity);
        if (result.Succeeded)
        {
            return ServiceResult<User>.Success(entity);
        }
        return ServiceResult<User>.Failure(result.Errors.Select(e => e.Description));
    }

    public async Task<ServiceResult> UpdateAsync(User entity)
    {
        var result = await _userManager.UpdateAsync(entity);
        if (result.Succeeded)
        {
            return ServiceResult.Success();
        }
        return ServiceResult.Failure(result.Errors.Select(e => e.Description));
    }

    public async Task<ServiceResult> DeleteAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return ServiceResult.Failure("User not found");

        var medicines = await _context.Medicines.Where(m => m.UserId == id).ToListAsync();
        foreach (var medicine in medicines)
        {
            medicine.IsDeleted = true;
            medicine.DeletedAt = DateTime.UtcNow;
        }

        var devices = await _context.UserDevices.Where(d => d.UserId == id).ToListAsync();
        foreach (var device in devices)
        {
            device.IsDeleted = true;
            device.DeletedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(CancellationToken.None);

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            return ServiceResult.Success();
        }
        return ServiceResult.Failure(result.Errors.Select(e => e.Description));
    }

    public async Task<ServiceResult<User>> GetByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user != null ? ServiceResult<User>.Success(user) : ServiceResult<User>.Failure("User not found");
    }
}