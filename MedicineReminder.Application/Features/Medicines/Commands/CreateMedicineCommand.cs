using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Domain.Entities;
using MedicineReminder.Domain.Enums;

namespace MedicineReminder.Application.Features.Medicines.Commands;

public record CreateMedicineCommand : IRequest<(int Data, string Message)>
{
    public string Name { get; init; } = string.Empty;
    public double DosageAmount { get; init; }
    public DosageUnit Unit { get; init; }
    public MedicineType Type { get; init; }
    public string? Description { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public class CreateMedicineCommandHandler : IRequestHandler<CreateMedicineCommand, (int Data, string Message)>
{
    private readonly IMedicineDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateMedicineCommandHandler(IMedicineDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<(int Data, string Message)> Handle(CreateMedicineCommand request, CancellationToken cancellationToken)
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

        return (entity.Id, "Medicine created successfully.");
    }
}
