using MediatR;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Application.Common.Models;
using MedicineReminder.Domain.Entities;

namespace MedicineReminder.Application.Features.Auth.Commands;

public record RegisterCommand(string Email, string Password) : IRequest<ServiceResult<User>>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ServiceResult<User>>
{
    private readonly IUserService _userService;

    public RegisterCommandHandler(IUserService userService)
    {
        _userService = userService;
    }

    public async Task<ServiceResult<User>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
        };

        return await _userService.CreateAsync(user);
    }
}