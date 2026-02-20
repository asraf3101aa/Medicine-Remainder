using Microsoft.EntityFrameworkCore;
using MedicineReminder.Domain.Entities;

namespace MedicineReminder.Application.Common.Interfaces;

public interface IMedicineDbContext
{
    DbSet<Medicine> Medicines { get; }
    DbSet<Reminder> Reminders { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
