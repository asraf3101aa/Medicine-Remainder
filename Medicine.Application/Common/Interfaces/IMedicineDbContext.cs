using Microsoft.EntityFrameworkCore;
using Medicine.Domain.Entities;

namespace Medicine.Application.Common.Interfaces;

public interface IMedicineDbContext
{
    DbSet<MedicineEntity> Medicines { get; }
    DbSet<Reminder> Reminders { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
