using MediatR;
using MedicineReminder.Application.Common.Interfaces;

namespace MedicineReminder.Application.Features.Auth.Commands;

public record VerifyEmailCommand(string UserId, string Token) : IRequest<(bool Success, string Message)>;

public class VerifyEmailCommandHandler : IRequestHandler<VerifyEmailCommand, (bool Success, string Message)>
{
    private readonly IIdentityService _identityService;

    public VerifyEmailCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<(bool Success, string Message)> Handle(VerifyEmailCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.VerifyEmailAsync(request.UserId, request.Token);
    }
}
