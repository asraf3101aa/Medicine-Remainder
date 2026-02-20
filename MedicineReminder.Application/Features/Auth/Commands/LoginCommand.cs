using MediatR;
using MedicineReminder.Application.Common.Interfaces;

namespace MedicineReminder.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password, string? FcmToken = null) : IRequest<(AuthData? Data, string Message, string[]? Errors)>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, (AuthData? Data, string Message, string[]? Errors)>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<(AuthData? Data, string Message, string[]? Errors)> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.LoginAsync(request.Email, request.Password, request.FcmToken);
    }
}
