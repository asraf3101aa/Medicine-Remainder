using System.Text;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Infrastructure.Identity;
using MedicineReminder.Infrastructure.Persistence;
using MedicineReminder.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using StackExchange.Redis;

namespace MedicineReminder.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<MedicineDbContext>(options =>
            options.UseNpgsql(connectionString,
                builder => builder.MigrationsAssembly(typeof(MedicineDbContext).Assembly.FullName)));

        services.AddScoped<IMedicineDbContext>(provider => provider.GetRequiredService<MedicineDbContext>());
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddTransient<IEmailService, EmailService>();
        services.AddTransient<ISmtpEmailSender, SmtpEmailSender>();
        services.AddHostedService<EmailBackgroundWorker>();
        services.AddTransient<INotificationService, FirebaseNotificationService>();

        // Redis & Workers
        var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost";

        if (redisConnectionString.StartsWith("redis://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(redisConnectionString);
            var password = uri.UserInfo.Contains(':') ? uri.UserInfo.Split(':')[1] : uri.UserInfo;
            redisConnectionString = $"{uri.Host}:{uri.Port},password={password}";
        }

        var options = ConfigurationOptions.Parse(redisConnectionString);
        options.AbortOnConnectFail = false;

        services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(options));
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddHostedService<DailyReminderLoaderWorker>();
        services.AddHostedService<ReminderPumperWorker>();

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<MedicineDbContext>()
        .AddDefaultTokenProviders();

        // JWT Configuration
        var jwtSettings = configuration.GetSection("JwtSettings");
        var secret = jwtSettings.GetValue<string>("Secret");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.GetValue<string>("Issuer"),
                ValidAudience = jwtSettings.GetValue<string>("Audience"),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!))
            };
        });

        return services;
    }
}
