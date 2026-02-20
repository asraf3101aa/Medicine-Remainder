using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedicineReminder.Application.Features.Medicines.Commands;

public record DeleteMedicineCommand(int Id) : IRequest<(bool Success, string Message)>;

public class DeleteMedicineCommandHandler : IRequestHandler<DeleteMedicineCommand, (bool Success, string Message)>
{
    private readonly IMedicineDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteMedicineCommandHandler(IMedicineDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<(bool Success, string Message)> Handle(DeleteMedicineCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var medicine = await _context.Medicines
            .FirstOrDefaultAsync(m => m.Id == request.Id && m.UserId == userId, cancellationToken);

        if (medicine == null)
        {
            return (false, "Medicine not found or you do not have permission.");
        }

        // Soft delete logic can be applied here, but for now we are doing hard delete
        // But the user requested "add up to disable the remainder except delete" previously for Reminders.
        // For Medicine, standard CRUD usually implies hard delete or soft delete.
        // Assuming hard delete for now as per requirement.
        _context.Medicines.Remove(medicine);
        await _context.SaveChangesAsync(cancellationToken);

        return (true, "Medicine deleted successfully.");
    }
}
