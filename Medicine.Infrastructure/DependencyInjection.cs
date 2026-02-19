using Medicine.Application.Common.Interfaces;
using Medicine.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Medicine.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MedicineDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection"),
                builder => builder.MigrationsAssembly(typeof(MedicineDbContext).Assembly.FullName)));

        services.AddScoped<IMedicineDbContext>(provider => provider.GetRequiredService<MedicineDbContext>());

        return services;
    }
}
