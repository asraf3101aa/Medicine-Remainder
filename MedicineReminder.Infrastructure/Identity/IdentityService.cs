using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Net;
using MedicineReminder.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MedicineReminder.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public IdentityService(UserManager<ApplicationUser> userManager, IConfiguration configuration, IEmailService emailService)
    {
        _userManager = userManager;
        _configuration = configuration;
        _emailService = emailService;
    }

    public async Task<(AuthData? Data, string Message, string[]? Errors)> LoginAsync(string email, string password, string? fcmToken = null)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return (null, "User not found", new[] { "User does not exist" });
        }

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            return (null, "Email not verified", new[] { "Please verify your email before logging in." });
        }

        if (!await _userManager.CheckPasswordAsync(user, password))
        {
            return (null, "Invalid credentials", new[] { "Invalid password" });
        }

        if (!string.IsNullOrEmpty(fcmToken) && user.FcmToken != fcmToken)
        {
            user.FcmToken = fcmToken;
            await _userManager.UpdateAsync(user);
        }

        return (GenerateAuthData(user), "User logged in successfully", null);
    }

    public async Task<(AuthData? Data, string Message, string[]? Errors)> RegisterAsync(string email, string password)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return (null, "Registration failed", new[] { "User with this email address already exists" });
        }

        var newUser = new ApplicationUser
        {
            Email = email,
            UserName = email
        };

        var createdUser = await _userManager.CreateAsync(newUser, password);
        if (!createdUser.Succeeded)
        {
            return (null, "Registration failed", createdUser.Errors.Select(x => x.Description).ToArray());
        }

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);
        var frontendUrl = _configuration.GetValue<string>("FrontendUrl") ?? "http://localhost:5173";
        var verificationUrl = $"{frontendUrl}/verify-email?userId={newUser.Id}&token={System.Net.WebUtility.UrlEncode(code)}";

        string emailBody = $@"
            <div style='font-family: Arial, sans-serif; text-align: center; padding: 20px; border: 1px solid #ddd; border-radius: 10px;'>
                <h2 style='color: #4CAF50;'>Welcome to Medicine Reminder!</h2>
                <p>Please click the button below to verify your email address and get started.</p>
                <a href='{verificationUrl}' style='display: inline-block; padding: 12px 24px; font-size: 16px; color: white; background-color: #4CAF50; text-decoration: none; border-radius: 5px; margin-top: 10px;'>Verify Email</a>
                <p style='margin-top: 20px; font-size: 12px; color: #777;'>If the button above doesn't work, copy and paste this link into your browser:</p>
                <p style='font-size: 12px; color: #777;'>{verificationUrl}</p>
            </div>";

        await _emailService.SendEmailAsync(newUser.Email!, "Verify your email", emailBody);

        return (null, "Registration successful. Please check your email to verify your account.", null);
    }

    public async Task<(AuthData? Data, string Message, string[]? Errors)> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        var principal = GetPrincipalFromToken(refreshToken, true); // Validate refresh token
        if (principal == null)
        {
            return (null, "Invalid token", new[] { "Invalid refresh token" });
        }

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userManager.FindByIdAsync(userId!);
        if (user == null)
        {
            return (null, "User not found", new[] { "User not found" });
        }

        return (GenerateAuthData(user), "Token refreshed successfully", null);
    }

    public async Task<(bool Success, string Message)> VerifyEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return (false, "User not found");

        var result = await _userManager.ConfirmEmailAsync(user, token);
        return result.Succeeded
            ? (true, "Email verified successfully")
            : (false, "Email verification failed");
    }

    private AuthData GenerateAuthData(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var accessExpiryMinutes = jwtSettings.GetValue<int>("ExpiryMinutes");
        var refreshExpiryDays = jwtSettings.GetValue<int>("RefreshTokenExpiryDays");

        var token = GenerateJwtToken(user, accessExpiryMinutes);
        var refreshToken = GenerateJwtToken(user, refreshExpiryDays * 24 * 60);

        return new AuthData(token, refreshToken);
    }

    private string GenerateJwtToken(ApplicationUser user, int expiryMinutes)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secret = jwtSettings.GetValue<string>("Secret");
        var issuer = jwtSettings.GetValue<string>("Issuer");
        var audience = jwtSettings.GetValue<string>("Audience");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
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
}
