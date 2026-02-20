namespace MedicineReminder.Application.Common.Interfaces;

public interface ICacheService
{
    Task AddReminderToHotSetAsync(string reminderId, DateTime reminderUtc);
    Task RemoveReminderFromHotSetAsync(string reminderId);
    Task<List<string>> GetDueRemindersAsync(DateTime utcNow);
}
