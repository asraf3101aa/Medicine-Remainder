using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Application.Common.Models;
using MedicineReminder.Domain.Entities;

namespace MedicineReminder.Application.Features.Auth.Commands;

public record LoginCommand(string Email, string Password, string? FcmToken = null, string? DeviceName = null) : IRequest<ServiceResult<AuthTokens>>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ServiceResult<AuthTokens>>
{
    private readonly IAuthService<User> _authService;

    public LoginCommandHandler(IAuthService<User> authService)
    {
        _authService = authService;
    }

    public async Task<ServiceResult<AuthTokens>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LoginAsync(request.Email, request.Password, request.FcmToken, request.DeviceName);
    }
}
