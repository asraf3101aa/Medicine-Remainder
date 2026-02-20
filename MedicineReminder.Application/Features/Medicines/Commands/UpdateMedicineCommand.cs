using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Domain.Entities;
using MedicineReminder.Domain.Enums;
using MedicineReminder.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace MedicineReminder.Application.Features.Medicines.Commands;

public record UpdateMedicineCommand : IRequest<ServiceResult<Medicine>>
{
    public string Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public double DosageAmount { get; init; }
    public DosageUnit Unit { get; init; }
    public MedicineType Type { get; init; }
    public string? Description { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public class UpdateMedicineCommandHandler : IRequestHandler<UpdateMedicineCommand, ServiceResult<Medicine>>
{
    private readonly IMedicineReminderDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateMedicineCommandHandler(IMedicineReminderDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ServiceResult<Medicine>> Handle(UpdateMedicineCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var medicine = await _context.Medicines
            .FirstOrDefaultAsync(m => m.Id == request.Id && m.UserId == userId, cancellationToken);

        if (medicine == null)
        {
            return ServiceResult<Medicine>.NotFound("Medicine not found or you do not have permission.");
        }

        medicine.Name = request.Name;
        medicine.DosageAmount = request.DosageAmount;
        medicine.Unit = request.Unit;
        medicine.Type = request.Type;
        medicine.Description = request.Description;
        medicine.StartDate = request.StartDate;
        medicine.EndDate = request.EndDate;

        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult<Medicine>.Success(medicine, "Medicine updated successfully.");
    }
}
