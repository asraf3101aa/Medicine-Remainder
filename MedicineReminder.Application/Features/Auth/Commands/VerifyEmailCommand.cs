using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Application.Common.Models;
using MedicineReminder.Domain.Entities;

namespace MedicineReminder.Application.Features.Auth.Commands;

public record VerifyEmailCommand(string UserId, string Token) : IRequest<ServiceResult<bool>>;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, ServiceResult<bool>>
{
    private readonly IAuthService<User> _authService;

    public VerifyEmailCommandHandler(IAuthService<User> authService)
    {
        _authService = authService;
    }

    public async Task<ServiceResult<bool>> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        return await _authService.VerifyEmailAsync(request.UserId, request.Token);
    }
}
