using MediatR;
using MedicineReminder.Application.Common.Interfaces;

namespace MedicineReminder.Application.Features.Auth.Commands;

public record RegisterCommand(string Email, string Password) : IRequest<(AuthData? Data, string Message, string[]? Errors)>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, (AuthData? Data, string Message, string[]? Errors)>
{
    private readonly IIdentityService _identityService;

    public RegisterCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<(AuthData? Data, string Message, string[]? Errors)> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        return await _identityService.RegisterAsync(request.Email, request.Password);
    }
}
