using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Domain.Entities;
using MedicineReminder.Domain.Enums;
using MedicineReminder.Application.Common.Models;

namespace MedicineReminder.Application.Features.Medicines.Commands;

public record CreateMedicineCommand : IRequest<ServiceResult<Medicine>>
{
    public string Name { get; init; } = string.Empty;
    public double DosageAmount { get; init; }
    public DosageUnit Unit { get; init; }
    public MedicineType Type { get; init; }
    public string? Description { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public class CreateMedicineCommandHandler : IRequestHandler<CreateMedicineCommand, ServiceResult<Medicine>>
{
    private readonly IMedicineReminderDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateMedicineCommandHandler(IMedicineReminderDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ServiceResult<Medicine>> Handle(CreateMedicineCommand request, CancellationToken cancellationToken)
    {
        var entity = new MedicineReminder.Domain.Entities.Medicine
        {
            Name = request.Name,
            DosageAmount = request.DosageAmount,
            Unit = request.Unit,
            Type = request.Type,
            Description = request.Description,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            UserId = _currentUserService.UserId ?? throw new UnauthorizedAccessException()
        };

        _context.Medicines.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult<Medicine>.Success(entity, "Medicine created successfully.");
    }
}
