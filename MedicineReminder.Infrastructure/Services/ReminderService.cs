using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Application.Common.Models;
using MedicineReminder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicineReminder.Infrastructure.Services;

public class ReminderService : IReminderService<Reminder>
{
    private readonly IMedicineReminderDbContext _context;

    public ReminderService(IMedicineReminderDbContext context)
    {
        _context = context;
    }
    // IEntityService implementations
    public async Task<ServiceResult<Reminder>> GetByIdAsync(string id)
    {
        var reminder = await _context.Reminders
            .Include(r => r.Medicine)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        return reminder != null ? ServiceResult<Reminder>.Success(reminder) : ServiceResult<Reminder>.Failure("Reminder not found");
    }

    public async Task<ServiceResult<PaginatedList<Reminder>>> GetAllAsync(PaginationQuery query)
    {
        var dbQuery = _context.Reminders
            .Include(r => r.Medicine)
            .Where(r => !r.IsDeleted)
            .OrderBy(r => r.ReminderUtc);

        return await CreatePaginatedResult(dbQuery, query);
    }

    public async Task<ServiceResult<Reminder>> CreateAsync(Reminder entity)
    {
        await _context.Reminders.AddAsync(entity);
        await _context.SaveChangesAsync(CancellationToken.None);
        return ServiceResult<Reminder>.Success(entity);
    }

    public async Task<ServiceResult> UpdateAsync(Reminder entity)
    {
        _context.Reminders.Update(entity);
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
        return ServiceResult.Failure("Reminder not found");
    }
    public async Task<ServiceResult<PaginatedList<Reminder>>> GetMedicineRemindersAsync(string medicineId, PaginationQuery query)
    {
        var dbQuery = _context.Reminders
            .Where(r => r.MedicineId == medicineId && !r.IsDeleted)
            .OrderBy(r => r.ReminderUtc);

        return await CreatePaginatedResult(dbQuery, query);
    }

    public async Task<ServiceResult<PaginatedList<Reminder>>> GetPendingRemindersAsync(string userId, PaginationQuery query)
    {
        var now = DateTime.UtcNow;

        var dbQuery = _context.Reminders
            .Include(r => r.Medicine)
            .Where(r => r.Medicine.UserId == userId &&
                        !r.IsDeleted &&
                        !r.IsTaken &&
                        r.IsActive &&
                        r.ReminderUtc <= now)
            .OrderBy(r => r.ReminderUtc);

        return await CreatePaginatedResult(dbQuery, query);
    }

    public async Task<ServiceResult<PaginatedList<Reminder>>> GetTodaysRemindersAsync(string userId, PaginationQuery query)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var dbQuery = _context.Reminders
            .Include(r => r.Medicine)
            .Where(r => r.Medicine.UserId == userId &&
                        !r.IsDeleted &&
                        r.IsActive &&
                        r.ReminderUtc >= today &&
                        r.ReminderUtc < tomorrow)
            .OrderBy(r => r.ReminderUtc);

        return await CreatePaginatedResult(dbQuery, query);
    }

    public async Task<ServiceResult<PaginatedList<Reminder>>> GetOverdueRemindersAsync(string userId, PaginationQuery query)
    {
        var now = DateTime.UtcNow;

        var dbQuery = _context.Reminders
            .Include(r => r.Medicine)
            .Where(r => r.Medicine.UserId == userId &&
                        !r.IsDeleted &&
                        !r.IsTaken &&
                        r.IsActive &&
                        r.ReminderUtc < now)
            .OrderBy(r => r.ReminderUtc);

        return await CreatePaginatedResult(dbQuery, query);
    }

    public async Task<ServiceResult<Reminder>> CreateReminderWithMedicineAsync(string medicineId, Reminder reminder)
    {
        var medicine = await _context.Medicines
            .FirstOrDefaultAsync(m => m.Id == medicineId && !m.IsDeleted);

        if (medicine == null)
            return ServiceResult<Reminder>.Failure("Medicine not found");

        reminder.MedicineId = medicineId;
        reminder.NextReminderUtc = reminder.ReminderUtc;

        await _context.Reminders.AddAsync(reminder);
        await _context.SaveChangesAsync(CancellationToken.None);

        return ServiceResult<Reminder>.Success(reminder);
    }

    public async Task<ServiceResult> MarkAsTakenAsync(string reminderId)
    {
        var reminder = await _context.Reminders
            .FirstOrDefaultAsync(r => r.Id == reminderId && !r.IsDeleted);

        if (reminder == null)
            return ServiceResult.Failure("Reminder not found");

        reminder.IsTaken = true;
        reminder.TakenAtUtc = DateTime.UtcNow;
        reminder.IsActive = false;

        await _context.SaveChangesAsync(CancellationToken.None);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> SnoozeReminderAsync(string reminderId, int snoozeMinutes)
    {
        var reminder = await _context.Reminders
            .FirstOrDefaultAsync(r => r.Id == reminderId && !r.IsDeleted);

        if (reminder == null || reminder.IsTaken)
            return ServiceResult.Failure("Reminder not found or already taken");

        reminder.SnoozeCount++;
        reminder.SnoozeDurationMinutes = snoozeMinutes;
        reminder.NextReminderUtc = DateTime.UtcNow.AddMinutes(snoozeMinutes);

        await _context.SaveChangesAsync(CancellationToken.None);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> ActivateReminderAsync(string reminderId)
    {
        var reminder = await _context.Reminders
            .FirstOrDefaultAsync(r => r.Id == reminderId && !r.IsDeleted);

        if (reminder == null)
            return ServiceResult.Failure("Reminder not found");

        reminder.IsActive = true;
        await _context.SaveChangesAsync(CancellationToken.None);

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeactivateReminderAsync(string reminderId)
    {
        var reminder = await _context.Reminders
            .FirstOrDefaultAsync(r => r.Id == reminderId && !r.IsDeleted);

        if (reminder == null)
            return ServiceResult.Failure("Reminder not found");

        reminder.IsActive = false;
        await _context.SaveChangesAsync(CancellationToken.None);

        return ServiceResult.Success();
    }

    private async Task<ServiceResult<PaginatedList<Reminder>>> CreatePaginatedResult(
        IQueryable<Reminder> query,
        PaginationQuery pagination)
    {
        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        var result = new PaginatedList<Reminder>(
            items,
            totalCount,
            pagination.PageNumber,
            pagination.PageSize);

        return ServiceResult<PaginatedList<Reminder>>.Success(result);
    }
}