using FluentValidation;
using MedicineReminder.Application.Common.Interfaces;
using MedicineReminder.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MedicineReminder.Application.Features.Auth.Commands;

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    private readonly UserManager<User> _userManager;

    public VerifyEmailCommandValidator(UserManager<User> userManager)
    {
        _userManager = userManager;

        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("UserId is required.")
            .MustAsync(BeValidUser).WithMessage("User not found.");

        RuleFor(v => v.Token)
            .NotEmpty().WithMessage("Token is required.");
    }

    private async Task<bool> BeValidUser(string userId, CancellationToken cancellationToken)
    {
        return await _userManager.FindByIdAsync(userId) != null;
    }
}
