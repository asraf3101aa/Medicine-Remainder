using FluentValidation;

namespace MedicineReminder.Application.Features.Auth.Commands;

public class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(v => v.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(v => v.Token)
            .NotEmpty().WithMessage("Token is required.");
    }
}
