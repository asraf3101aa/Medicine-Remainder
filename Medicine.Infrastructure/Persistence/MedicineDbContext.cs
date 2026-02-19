using Medicine.Application.Common.Interfaces;
using Medicine.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Medicine.Infrastructure.Persistence;

public class MedicineDbContext : DbContext, IMedicineDbContext
{
    public MedicineDbContext(DbContextOptions<MedicineDbContext> options) : base(options)
    {
    }

    public DbSet<MedicineEntity> Medicines => Set<MedicineEntity>();
    public DbSet<Reminder> Reminders => Set<Reminder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MedicineDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
