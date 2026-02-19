using FluentAssertions;
using Moq;
using Medicine.Application.Common.Interfaces;
using Medicine.Application.Features.Medicines.Commands;
using Medicine.Domain.Entities;
using Medicine.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Medicine.Application.UnitTests.Features.Medicines.Commands;

public class CreateMedicineCommandHandlerTests
{
    private readonly Mock<IMedicineDbContext> _contextMock;
    private readonly CreateMedicineCommandHandler _handler;

    public CreateMedicineCommandHandlerTests()
    {
        _contextMock = new Mock<IMedicineDbContext>();

        // Mock the Medicines DbSet
        var medicines = new List<MedicineEntity>().AsQueryable();
        var mockSet = new Mock<DbSet<MedicineEntity>>();

        _contextMock.Setup(m => m.Medicines).Returns(mockSet.Object);

        _handler = new CreateMedicineCommandHandler(_contextMock.Object);
    }

    [Fact]
    public async Task Handle_Should_AddMedicineToDatabase()
    {
        // Arrange
        var command = new CreateMedicineCommand
        {
            Name = "Paracetamol",
            DosageAmount = 500,
            Unit = DosageUnit.Mg,
            UserEmail = "test@example.com"
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _contextMock.Verify(m => m.Medicines.Add(It.IsAny<MedicineEntity>()), Times.Once);
        _contextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
