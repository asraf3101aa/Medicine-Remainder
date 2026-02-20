using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Application.Common.Models;
using MedicineReminder.Domain.Entities;
using MedicineReminder.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace MedicineReminder.Infrastructure.UnitTests.Services;

public class AuthServiceTests
{
    private readonly Mock<IMedicineReminderDbContext> _contextMock;
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _contextMock = new Mock<IMedicineReminderDbContext>();
        _userManagerMock = MockUserManager<User>();
        _configurationMock = new Mock<IConfiguration>();
        _emailServiceMock = new Mock<IEmailService>();

        _authService = new AuthService(
            _contextMock.Object,
            _userManagerMock.Object,
            _configurationMock.Object,
            _emailServiceMock.Object);
    }

    private Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
    {
        var store = new Mock<IUserStore<TUser>>();
        return new Mock<UserManager<TUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnSuccess_WhenUserIsCreated()
    {
        // Arrange
        var user = new User { Email = "test@example.com", UserName = "test@example.com" };
        _userManagerMock.Setup(m => m.CreateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.RegisterAsync(user);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(user);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnAuthTokens_WhenCredentialsAreValid()
    {
        // Arrange
        var email = "test@example.com";
        var password = "Password123!";
        var user = new User { Id = "user-id", Email = email };

        _userManagerMock.Setup(m => m.FindByEmailAsync(email))
            .ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, password))
            .ReturnsAsync(true);

        var jwtSettings = new Mock<IConfigurationSection>();
        jwtSettings.Setup(s => s.GetSection("Secret")).Returns(new Mock<IConfigurationSection>().Object);
        _configurationMock.Setup(c => c.GetSection("JwtSettings")).Returns(SetupJwtConfig());

        // Act
        var result = await _authService.LoginAsync(email, password);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.AccessToken.Should().NotBeEmpty();
    }

    private IConfigurationSection SetupJwtConfig()
    {
        var config = new Dictionary<string, string>
        {
            {"Secret", "very-long-secret-key-at-least-32-chars-long"},
            {"Issuer", "test-issuer"},
            {"Audience", "test-audience"},
            {"ExpiryMinutes", "60"}
        };

        var mockSection = new Mock<IConfigurationSection>();
        foreach (var kvp in config)
        {
            var subSection = new Mock<IConfigurationSection>();
            subSection.Setup(s => s.Value).Returns(kvp.Value);
            mockSection.Setup(s => s.GetSection(kvp.Key)).Returns(subSection.Object);
        }

        return mockSection.Object;
    }
}
