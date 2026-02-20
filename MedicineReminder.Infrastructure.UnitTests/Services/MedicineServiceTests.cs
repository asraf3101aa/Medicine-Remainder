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

public class MedicineServiceTests
{
    private readonly MedicineReminderDbContext _context;
    private readonly MedicineService _medicineService;

    public MedicineServiceTests()
    {
        var options = new DbContextOptionsBuilder<MedicineReminderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MedicineReminderDbContext(options);
        _medicineService = new MedicineService(_context);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddMedicine()
    {
        // Arrange
        var medicine = new Medicine { Id = "med-1", Name = "Aspirin", UserId = "user-1" };

        // Act
        var result = await _medicineService.CreateAsync(medicine);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _context.Medicines.Count().Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMedicine_WhenExists()
    {
        // Arrange
        var medicine = new Medicine { Id = "med-1", Name = "Aspirin", UserId = "user-1" };
        _context.Medicines.Add(medicine);
        await _context.SaveChangesAsync();

        // Act
        var result = await _medicineService.GetByIdAsync("med-1");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Name.Should().Be("Aspirin");
    }

    [Fact]
    public async Task GetUserMedicinesAsync_ShouldReturnOnlyUserMedicines()
    {
        // Arrange
        _context.Medicines.AddRange(new List<Medicine>
        {
            new Medicine { Id = "m1", UserId = "u1", Name = "Med 1" },
            new Medicine { Id = "m2", UserId = "u1", Name = "Med 2" },
            new Medicine { Id = "m3", UserId = "u2", Name = "Med 3" }
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _medicineService.GetUserMedicinesAsync("u1", new PaginationQuery { PageNumber = 1, PageSize = 10 });

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data!.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task DeleteAsync_ShouldPerformSoftDelete()
    {
        // Arrange
        var medicine = new Medicine { Id = "med-1", Name = "Aspirin", UserId = "user-1" };
        _context.Medicines.Add(medicine);
        await _context.SaveChangesAsync();

        // Act
        var result = await _medicineService.DeleteAsync("med-1");

        // Assert
        result.IsSuccess.Should().BeTrue();
        var deletedMed = await _context.Medicines.IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.Id == "med-1");
        deletedMed.IsDeleted.Should().BeTrue();
        deletedMed.DeletedAt.Should().NotBeNull();
    }
}
