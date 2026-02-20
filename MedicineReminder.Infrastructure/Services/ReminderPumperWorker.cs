using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MedicineReminder.Infrastructure.Services;

public class ReminderPumperWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReminderPumperWorker> _logger;
    private readonly ICacheService _cacheService;

    public ReminderPumperWorker(IServiceProvider serviceProvider, ILogger<ReminderPumperWorker> logger, ICacheService cacheService)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _cacheService = cacheService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReminderPumperWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Pull due reminders from Redis
                // Check if any reminders are due
                var reminderIds = await _cacheService.GetDueRemindersAsync(DateTime.UtcNow);

                if (reminderIds.Any())
                {
                    _logger.LogInformation("Pumped {Count} due reminders from Redis.", reminderIds.Count);

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var context = scope.ServiceProvider.GetRequiredService<MedicineDbContext>();
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                        var reminders = await context.Reminders
                            .Include(r => r.Medicine)
                            .Where(r => reminderIds.Contains(r.Id))
                            .ToListAsync(stoppingToken);

                        foreach (var reminder in reminders)
                        {
                            var user = await context.Users.FindAsync(new object[] { reminder.Medicine.UserId }, stoppingToken);

                            if (user != null)
                            {
                                var subject = "Medicine Reminder";
                                var body = $"It's time to take your medicine: {reminder.Medicine.Name} ({reminder.Medicine.DosageAmount} {(MedicineReminder.Domain.Enums.DosageUnit)reminder.Medicine.Unit})";

                                // Send Push Notification
                                if (!string.IsNullOrEmpty(user.FcmToken))
                                {
                                    await notificationService.SendNotificationAsync(subject, body, user.Id);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in ReminderPumperWorker.");
            }

            // Run every minute
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}
