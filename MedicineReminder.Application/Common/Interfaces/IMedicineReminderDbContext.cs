using Microsoft.EntityFrameworkCore;
using MedicineReminder.Domain.Entities;

namespace MedicineReminder.Application.Common.Interfaces;

public interface IMedicineReminderDbContext
{
    DbSet<Medicine> Medicines { get; }
    DbSet<Reminder> Reminders { get; }
    DbSet<UserDevice> UserDevices { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
