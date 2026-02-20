using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Application.Common.Models;
using MedicineReminder.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicineReminder.Application.Features.Medicines.Commands;

public record DeleteMedicineCommand(string Id) : IRequest<ServiceResult<Medicine>>;

public class DeleteMedicineCommandHandler : IRequestHandler<DeleteMedicineCommand, ServiceResult<Medicine>>
{
    private readonly IMedicineReminderDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteMedicineCommandHandler(IMedicineReminderDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<ServiceResult<Medicine>> Handle(DeleteMedicineCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var medicine = await _context.Medicines
            .FirstOrDefaultAsync(m => m.Id == request.Id && m.UserId == userId, cancellationToken);

        if (medicine == null)
        {
            return ServiceResult<Medicine>.NotFound("Medicine not found or you do not have permission.");
        }

        _context.Medicines.Remove(medicine);
        await _context.SaveChangesAsync(cancellationToken);

        return ServiceResult<Medicine>.Success(medicine, "Medicine deleted successfully.");
    }
}
