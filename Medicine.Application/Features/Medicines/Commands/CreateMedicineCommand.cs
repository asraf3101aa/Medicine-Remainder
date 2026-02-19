using MediatR;
using Medicine.Application.Common.Interfaces;
using Medicine.Domain.Entities;
using Medicine.Domain.Enums;

namespace Medicine.Application.Features.Medicines.Commands;

public record CreateMedicineCommand : IRequest<int>
{
    public string Name { get; init; } = string.Empty;
    public double DosageAmount { get; init; }
    public DosageUnit Unit { get; init; }
    public string? Description { get; init; }
    public string UserEmail { get; init; } = string.Empty;
}

public class CreateMedicineCommandHandler : IRequestHandler<CreateMedicineCommand, int>
{
    private readonly IMedicineDbContext _context;

    public CreateMedicineCommandHandler(IMedicineDbContext context)
    {
        _context = context;
    }

    public async Task<int> Handle(CreateMedicineCommand request, CancellationToken cancellationToken)
    {
        var entity = new MedicineEntity
        {
            Name = request.Name,
            DosageAmount = request.DosageAmount,
            Unit = request.Unit,
            Description = request.Description,
            UserEmail = request.UserEmail
        };

        _context.Medicines.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}
