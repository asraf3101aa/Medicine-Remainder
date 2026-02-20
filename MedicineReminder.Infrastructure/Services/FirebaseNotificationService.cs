using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MedicineReminder.Infrastructure.Services;

public class FirebaseNotificationService : IFirebaseNotificationService
{
    private readonly ILogger<FirebaseNotificationService> _logger;
    private readonly MedicineReminderDbContext _dbContext;

    public FirebaseNotificationService(ILogger<FirebaseNotificationService> logger, MedicineReminderDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;

        if (FirebaseApp.DefaultInstance == null)
        {
            try
            {
                // Attempt to initialize Firebase with application default credentials
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

        var userDevices = await _dbContext.UserDevices
            .Where(ud => ud.UserId == userId && !string.IsNullOrEmpty(ud.FcmToken))
            .ToListAsync();

        if (userDevices.Count == 0)
        {
            _logger.LogWarning("User {UserId} has no devices with valid FCM tokens.", userId);
            return;
        }

        foreach (var device in userDevices)
        {
            var message = new Message
            {
                Token = device.FcmToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                }
            };

            try
            {
                string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation("Successfully sent message to device {DeviceId}: {Response}", device.Id, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending Firebase notification to device {DeviceId} for user {UserId}.", device.Id, userId);
            }
        }
    }
}
