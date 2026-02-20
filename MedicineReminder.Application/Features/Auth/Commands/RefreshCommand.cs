using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Application.Common.Models;
using MedicineReminder.Domain.Entities;

namespace MedicineReminder.Application.Features.Auth.Commands;

public record RefreshCommand(string AccessToken, string RefreshToken) : IRequest<ServiceResult<AuthTokens>>;

public class RefreshCommandHandler : IRequestHandler<RefreshCommand, ServiceResult<AuthTokens>>
{
    private readonly IAuthService<User> _authService;

    public RefreshCommandHandler(IAuthService<User> authService)
    {
        _authService = authService;
    }

    public async Task<ServiceResult<AuthTokens>> Handle(RefreshCommand request, CancellationToken cancellationToken)
    {
        return await _authService.RefreshTokensAsync(request.RefreshToken);
    }
}
