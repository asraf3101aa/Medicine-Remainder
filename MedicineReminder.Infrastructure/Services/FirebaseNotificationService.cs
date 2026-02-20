using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using MedicineReminder.Infrastructure.Identity;
using MedicineReminder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MedicineReminder.Infrastructure.Services;

public interface INotificationService
{
    Task SendNotificationAsync(string title, string body, string userId);
    Task SendMulticastNotificationAsync(string title, string body, List<string> userIds);
}

public class FirebaseNotificationService : INotificationService
{
    private readonly ILogger<FirebaseNotificationService> _logger;
    private readonly MedicineDbContext _dbContext;

    public FirebaseNotificationService(ILogger<FirebaseNotificationService> logger, MedicineDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;

        if (FirebaseApp.DefaultInstance == null)
        {
            try
            {
                // This will fail if GOOGLE_APPLICATION_CREDENTIALS is not set.
                // For development, you can suppress it or mock it.
                // We'll wrap it in a try-catch to not crash the app start, but warn heavily.
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.GetApplicationDefault()
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to initialize FirebaseApp: {Message}. Notifications will be disabled.", ex.Message);
            }
        }
    }

    public async Task SendNotificationAsync(string title, string body, string userId)
    {
        if (FirebaseMessaging.DefaultInstance == null)
        {
            _logger.LogWarning("FirebaseMessaging is not initialized. Skipping notification for {UserId}.", userId);
            return;
        }

        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.FcmToken))
        {
            _logger.LogWarning("User {UserId} not found or has no FCM token.", userId);
            return;
        }

        var message = new Message
        {
            Token = user.FcmToken,
            Notification = new Notification
            {
                Title = title,
                Body = body
            }
        };

        try
        {
            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
            _logger.LogInformation("Successfully sent message: {Response}", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Firebase notification to user {UserId}.", userId);
        }
    }

    public async Task SendMulticastNotificationAsync(string title, string body, List<string> userIds)
    {
        var tokens = await _dbContext.Users
            .Where(u => userIds.Contains(u.Id) && !string.IsNullOrEmpty(u.FcmToken))
            .Select(u => u.FcmToken!)
            .ToListAsync();

        if (!tokens.Any())
        {
            _logger.LogWarning("No valid FCM tokens found for the provided user IDs.");
            return;
        }

        var message = new MulticastMessage
        {
            Tokens = tokens,
            Notification = new Notification
            {
                Title = title,
                Body = body
            }
        };

        try
        {
            BatchResponse response = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
            _logger.LogInformation("{Count} messages sent successfully.", response.SuccessCount);

            if (response.FailureCount > 0)
            {
                _logger.LogWarning("{Count} messages failed to send.", response.FailureCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending multicast Firebase notifications.");
        }
    }
}
