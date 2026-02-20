using System.Linq.Expressions;
using FluentAssertions;
using MedicineReminder.Application.Common.Models;
using MedicineReminder.Domain.Entities;
using MedicineReminder.Infrastructure.Persistence;
using MedicineReminder.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace MedicineReminder.Infrastructure.UnitTests.Services;

public class ReminderServiceTests
{
    private readonly MedicineReminderDbContext _context;
    private readonly ReminderService _reminderService;

    public ReminderServiceTests()
    {
        var options = new DbContextOptionsBuilder<MedicineReminderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MedicineReminderDbContext(options);
        _reminderService = new ReminderService(_context);
    }

    [Fact]
    public async Task GetPendingRemindersAsync_ShouldReturnUpcomingReminders()
    {
        // Arrange
        var userId = "u1";
        var medicine = new Medicine { Id = "m1", UserId = userId, Name = "Med 1" };
        _context.Medicines.Add(medicine);

        var now = DateTime.UtcNow;
        _context.Reminders.AddRange(new List<Reminder>
        {
            new Reminder { Id = "r1", MedicineId = "m1", ReminderUtc = now.AddMinutes(-5), IsActive = true, IsTaken = false },
            new Reminder { Id = "r2", MedicineId = "m1", ReminderUtc = now.AddMinutes(5), IsActive = true, IsTaken = false },
            new Reminder { Id = "r3", MedicineId = "m1", ReminderUtc = now.AddMinutes(-10), IsActive = true, IsTaken = true }
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _reminderService.GetPendingRemindersAsync(userId, new PaginationQuery { PageNumber = 1, PageSize = 10 });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items.First().Id.Should().Be("r1");
    }

    [Fact]
    public async Task MarkAsTakenAsync_ShouldUpdateStatus()
    {
        // Arrange
        var reminder = new Reminder { Id = "r1", IsTaken = false, IsActive = true };
        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync();

        // Act
        var result = await _reminderService.MarkAsTakenAsync("r1");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updated = await _context.Reminders.FindAsync("r1");
        updated.IsTaken.Should().BeTrue();
        updated.IsActive.Should().BeFalse();
        updated.TakenAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task SnoozeReminderAsync_ShouldUpdateNextReminder()
    {
        // Arrange
        var reminder = new Reminder { Id = "r1", ReminderUtc = DateTime.UtcNow, IsActive = true };
        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync();

        // Act
        var result = await _reminderService.SnoozeReminderAsync("r1", 15);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var updated = await _context.Reminders.FindAsync("r1");
        updated.SnoozeCount.Should().Be(1);
        updated.NextReminderUtc.Should().BeAfter(DateTime.UtcNow.AddMinutes(14));
    }
}
