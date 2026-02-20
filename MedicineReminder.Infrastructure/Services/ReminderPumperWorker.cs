using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Domain.Entities;
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
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var dueReminderIds = await _cacheService.GetDueRemindersAsync(DateTime.UtcNow);

                if (dueReminderIds != null && dueReminderIds.Any())
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var notificationService = scope.ServiceProvider.GetRequiredService<IFirebaseNotificationService>();
                        var reminderService = scope.ServiceProvider.GetRequiredService<IReminderService<Reminder>>();

                        foreach (var reminderId in dueReminderIds)
                        {
                            var result = await reminderService.GetByIdAsync(reminderId);
                            if (result.IsSuccess && result.Data != null)
                            {
                                var reminder = result.Data;
                                await notificationService.SendNotificationAsync(
                                    $"Reminder: {reminder.Medicine.Name}",
                                    $"It's time to take your {reminder.Medicine.Name} ({reminder.Medicine.DosageAmount} {reminder.Medicine.Unit})",
                                    reminder.Medicine.UserId);

                                _logger.LogInformation("Sent notification for reminder {ReminderId}", reminderId);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while pumping reminders.");
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}