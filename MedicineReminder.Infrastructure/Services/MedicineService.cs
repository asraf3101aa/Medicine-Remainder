using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Application.Common.Models;
using MedicineReminder.Domain.Entities;
using MedicineReminder.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedicineReminder.Infrastructure.Services;

public class MedicineService : IMedicineService
{
    private readonly IMedicineReminderDbContext _context;

    public MedicineService(IMedicineReminderDbContext context)
    {
        _context = context;
    }

    // IEntityService implementations
    public async Task<ServiceResult<Medicine>> GetByIdAsync(string id)
    {
        var medicine = await _context.Medicines
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

        return medicine != null ? ServiceResult<Medicine>.Success(medicine) : ServiceResult<Medicine>.Failure("Medicine not found");
    }

    public async Task<ServiceResult<PaginatedList<Medicine>>> GetAllAsync(PaginationQuery query)
    {
        var dbQuery = _context.Medicines
            .Where(m => !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt);

        return await CreatePaginatedResult(dbQuery, query);
    }

    public async Task<ServiceResult<Medicine>> CreateAsync(Medicine entity)
    {
        await _context.Medicines.AddAsync(entity);
        await _context.SaveChangesAsync(CancellationToken.None);

        return ServiceResult<Medicine>.Success(entity);
    }

    public async Task<ServiceResult> UpdateAsync(Medicine entity)
    {
        _context.Medicines.Update(entity);
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
        return ServiceResult.Failure("Medicine not found");
    }

    // IMedicineService implementations
    public async Task<ServiceResult<PaginatedList<Medicine>>> GetUserMedicinesAsync(string userId, PaginationQuery query)
    {
        var dbQuery = _context.Medicines
            .Where(m => m.UserId == userId && !m.IsDeleted)
            .Include(m => m.Reminders.Where(r => !r.IsDeleted))
            .OrderByDescending(m => m.CreatedAt);

        return await CreatePaginatedResult(dbQuery, query);
    }

    public async Task<ServiceResult<PaginatedList<Medicine>>> GetActiveMedicinesAsync(string userId, PaginationQuery query)
    {
        var now = DateTime.UtcNow;
        var dbQuery = _context.Medicines
            .Where(m => m.UserId == userId &&
                       !m.IsDeleted &&
                       m.StartDate <= now &&
                       (!m.EndDate.HasValue || m.EndDate >= now))
            .Include(m => m.Reminders.Where(r => !r.IsDeleted && r.IsActive))
            .OrderByDescending(m => m.CreatedAt);

        return await CreatePaginatedResult(dbQuery, query);
    }

    public async Task<ServiceResult<PaginatedList<Medicine>>> GetMedicinesByTypeAsync(string userId, MedicineType type, PaginationQuery query)
    {
        var dbQuery = _context.Medicines
            .Where(m => m.UserId == userId &&
                       m.Type == type &&
                       !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt);

        return await CreatePaginatedResult(dbQuery, query);
    }

    public async Task<ServiceResult<bool>> IsMedicineOwnedByUserAsync(string medicineId, string userId)
    {
        var owned = await _context.Medicines
            .AnyAsync(m => m.Id == medicineId &&
                           m.UserId == userId &&
                           !m.IsDeleted);
        return ServiceResult<bool>.Success(owned);
    }

    public async Task<ServiceResult<PaginatedList<Medicine>>> GetMedicinesWithRemindersAsync(string userId, PaginationQuery query)
    {
        var dbQuery = _context.Medicines
            .Where(m => m.UserId == userId && !m.IsDeleted)
            .Include(m => m.Reminders.Where(r => !r.IsDeleted))
            .OrderByDescending(m => m.CreatedAt);

        return await CreatePaginatedResult(dbQuery, query);
    }

    public async Task<ServiceResult<Medicine>> GetMedicineWithRemindersAsync(string medicineId)
    {
        var medicine = await _context.Medicines
            .Include(m => m.Reminders.Where(r => !r.IsDeleted))
            .FirstOrDefaultAsync(m => m.Id == medicineId && !m.IsDeleted);

        return medicine != null ? ServiceResult<Medicine>.Success(medicine) : ServiceResult<Medicine>.Failure("Medicine not found");
    }

    public async Task<ServiceResult<int>> GetUserMedicinesCountAsync(string userId)
    {
        var count = await _context.Medicines
            .CountAsync(m => m.UserId == userId && !m.IsDeleted);
        return ServiceResult<int>.Success(count);
    }

    private async Task<ServiceResult<PaginatedList<Medicine>>> CreatePaginatedResult(
        IQueryable<Medicine> query,
        PaginationQuery pagination)
    {
        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        var result = new PaginatedList<Medicine>(
            items,
            totalCount,
            pagination.PageNumber,
            pagination.PageSize);

        return ServiceResult<PaginatedList<Medicine>>.Success(result);
    }
}