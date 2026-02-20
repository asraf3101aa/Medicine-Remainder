using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Domain.Entities;
using MedicineReminder.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedicineReminder.Application.Features.Medicines.Commands;

public record UpdateMedicineCommand : IRequest<(bool Success, string Message)>
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public double DosageAmount { get; init; }
    public DosageUnit Unit { get; init; }
    public MedicineType Type { get; init; }
    public string? Description { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public class UpdateMedicineCommandHandler : IRequestHandler<UpdateMedicineCommand, (bool Success, string Message)>
{
    private readonly IMedicineDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateMedicineCommandHandler(IMedicineDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<(bool Success, string Message)> Handle(UpdateMedicineCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var medicine = await _context.Medicines
            .FirstOrDefaultAsync(m => m.Id == request.Id && m.UserId == userId, cancellationToken);

        if (medicine == null)
        {
            return (false, "Medicine not found or you do not have permission.");
        }

        medicine.Name = request.Name;
        medicine.DosageAmount = request.DosageAmount;
        medicine.Unit = request.Unit;
        medicine.Type = request.Type;
        medicine.Description = request.Description;
        medicine.StartDate = request.StartDate;
        medicine.EndDate = request.EndDate;

        await _context.SaveChangesAsync(cancellationToken);

        return (true, "Medicine updated successfully.");
    }
}
