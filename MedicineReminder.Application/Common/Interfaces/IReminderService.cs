using MedicineReminder.Application.Common.Models;

namespace MedicineReminder.Application.Common.Interfaces;

public interface IReminderService<T> : IEntityService<T> where T : class
{
    Task<ServiceResult<PaginatedList<T>>> GetMedicineRemindersAsync(string medicineId, PaginationQuery query);
    Task<ServiceResult<PaginatedList<T>>> GetPendingRemindersAsync(string userId, PaginationQuery query);
    Task<ServiceResult<PaginatedList<T>>> GetTodaysRemindersAsync(string userId, PaginationQuery query);
    Task<ServiceResult<PaginatedList<T>>> GetOverdueRemindersAsync(string userId, PaginationQuery query);
    Task<ServiceResult> MarkAsTakenAsync(string reminderId);
    Task<ServiceResult> SnoozeReminderAsync(string reminderId, int snoozeMinutes);
    Task<ServiceResult<T>> CreateReminderWithMedicineAsync(string medicineId, T reminder);
    Task<ServiceResult> ActivateReminderAsync(string reminderId);
    Task<ServiceResult> DeactivateReminderAsync(string reminderId);
}