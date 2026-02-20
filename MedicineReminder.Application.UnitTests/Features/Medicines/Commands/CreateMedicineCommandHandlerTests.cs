using FluentAssertions;
using Moq;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Application.Features.Medicines.Commands;
using MedicineReminder.Domain.Entities;
using MedicineReminder.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedicineReminder.Application.UnitTests.Features.Medicines.Commands;

public class CreateMedicineCommandHandlerTests
{
    private readonly Mock<IMedicineDbContext> _contextMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CreateMedicineCommandHandler _handler;

    public CreateMedicineCommandHandlerTests()
    {
        _contextMock = new Mock<IMedicineDbContext>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        // Mock the Medicines DbSet
        var mockSet = new Mock<DbSet<MedicineReminder.Domain.Entities.Medicine>>();

        _contextMock.Setup(m => m.Medicines).Returns(mockSet.Object);
        _currentUserServiceMock.Setup(m => m.UserId).Returns("test-user-id");

        _handler = new CreateMedicineCommandHandler(_contextMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_AddMedicineToDatabase()
    {
        // Arrange
        var command = new CreateMedicineCommand
        {
            Name = "Paracetamol",
            DosageAmount = 500,
            Unit = DosageUnit.Mg
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _contextMock.Verify(m => m.Medicines.Add(It.IsAny<MedicineReminder.Domain.Entities.Medicine>()), Times.Once);
        _contextMock.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
