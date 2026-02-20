namespace MedicineReminder.Application.Common.Interfaces;

public interface IFirebaseNotificationService
{
    Task SendNotificationAsync(string title, string body, string userId);
}