using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MedicineReminder.Infrastructure.Services;

public class DailyReminderLoaderWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DailyReminderLoaderWorker> _logger;
    private readonly ICacheService _cacheService;

    public DailyReminderLoaderWorker(IServiceProvider serviceProvider, ILogger<DailyReminderLoaderWorker> logger, ICacheService cacheService)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _cacheService = cacheService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("DailyReminderLoaderWorker running at: {Time}", DateTimeOffset.Now);

            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<MedicineReminderDbContext>();

                    var next24Hours = DateTime.UtcNow.AddHours(24);

                    var upcomingReminders = await context.Reminders
                        .Where(r => r.IsActive && !r.IsTaken && r.NextReminderUtc <= next24Hours)
                        .Select(r => new { r.Id, r.NextReminderUtc })
                        .ToListAsync(stoppingToken);

                    foreach (var reminder in upcomingReminders)
                    {
                        await _cacheService.AddReminderToHotSetAsync(reminder.Id, reminder.NextReminderUtc);
                    }

                    _logger.LogInformation("Loaded {Count} upcoming reminders into Redis hot set.", upcomingReminders.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while loading daily reminders.");
            }

            await Task.Delay(TimeSpan.FromHours(4), stoppingToken);
        }
    }
}
