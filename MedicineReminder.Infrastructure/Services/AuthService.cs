using MedicineReminder.Application.Common.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MedicineReminder.Application.Common.Models;
using MedicineReminder.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MedicineReminder.Infrastructure.Services;

public class AuthService : IAuthService<User>
{
    private readonly IMedicineReminderDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public AuthService(IMedicineReminderDbContext context, UserManager<User> userManager, IConfiguration configuration, IEmailService emailService)
    {
        _userManager = userManager;
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
    }

    public async Task<ServiceResult<User>> RegisterAsync(User user)
    {
        var result = await _userManager.CreateAsync(user);
        if (!result.Succeeded)
        {
            return ServiceResult<User>.Failure(result.Errors.Select(e => e.Description));
        }
        return ServiceResult<User>.Success(user);
    }

    public async Task<ServiceResult<AuthTokens>> LoginAsync(string email, string password, string? fcmToken = null, string? deviceName = null)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, password))
        {
            return ServiceResult<AuthTokens>.Failure("Invalid email or password");
        }

        if (!string.IsNullOrEmpty(fcmToken))
        {
            var userDevice = new UserDevice
            {
                UserId = user.Id,
                FcmToken = fcmToken,
                DeviceName = deviceName ?? "Unknown"
            };
            await _context.UserDevices.AddAsync(userDevice);
            await _context.SaveChangesAsync();
        }

        return ServiceResult<AuthTokens>.Success(GenerateTokens(user));
    }

    public async Task<ServiceResult<AuthTokens>> RefreshTokensAsync(string refreshToken)
    {
        var principal = GetPrincipalFromToken(refreshToken, true);
        if (principal == null)
        {
            return ServiceResult<AuthTokens>.Failure("Invalid refresh token");
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId!);
        if (user == null)
        {
            return ServiceResult<AuthTokens>.Failure("User not found");
        }

        return ServiceResult<AuthTokens>.Success(GenerateTokens(user));
    }

    public async Task<ServiceResult<bool>> VerifyEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return ServiceResult<bool>.Failure("User not found");

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (result.Succeeded)
        {
            return ServiceResult<bool>.Success(true);
        }
        return ServiceResult<bool>.Failure(result.Errors.Select(e => e.Description));
    }

    private ClaimsPrincipal? GetPrincipalFromToken(string token, bool validateLifetime)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secret = jwtSettings.GetValue<string>("Secret");

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!)),
            ValidateLifetime = validateLifetime
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }
    private AuthTokens GenerateTokens(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secret = jwtSettings.GetValue<string>("Secret");
        var issuer = jwtSettings.GetValue<string>("Issuer");
        var audience = jwtSettings.GetValue<string>("Audience");
        var expiryMinutes = jwtSettings.GetValue<int>("ExpiryMinutes", 60);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        var refreshToken = Guid.NewGuid().ToString();

        return new AuthTokens
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }
}