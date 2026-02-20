using MediatR;
using MedicineReminder.Application.Common.Interfaces;

namespace MedicineReminder.Application.Features.Auth.Commands;

public record RefreshCommand(string AccessToken, string RefreshToken) : IRequest<(AuthData? Data, string Message, string[]? Errors)>;

public class RefreshCommandHandler : IRequestHandler<RefreshCommand, (AuthData? Data, string Message, string[]? Errors)>
{
    private readonly IIdentityService _identityService;

    public RefreshCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<(AuthData? Data, string Message, string[]? Errors)> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.RefreshTokenAsync(request.AccessToken, request.RefreshToken);
    }
}
