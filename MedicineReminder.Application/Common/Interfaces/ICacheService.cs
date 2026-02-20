namespace MedicineReminder.Application.Common.Interfaces;

public interface ICacheService
{
    Task AddReminderToHotSetAsync(int reminderId, DateTime reminderUtc);
    Task RemoveReminderFromHotSetAsync(int reminderId);
    Task<List<int>> GetDueRemindersAsync(DateTime utcNow);
}
