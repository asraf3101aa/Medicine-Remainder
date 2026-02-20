using FluentValidation;

namespace MedicineReminder.Application.Features.Auth.Commands;

public class RefreshCommandValidator : AbstractValidator<RefreshCommand>
{
    public RefreshCommandValidator()
    {
        RuleFor(v => v.RefreshToken)
            .NotEmpty().WithMessage("RefreshToken is required.");
    }
}
