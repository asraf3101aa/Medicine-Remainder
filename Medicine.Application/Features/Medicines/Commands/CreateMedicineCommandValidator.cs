using FluentValidation;

namespace Medicine.Application.Features.Medicines.Commands;

public class CreateMedicineCommandValidator : AbstractValidator<CreateMedicineCommand>
{
    public CreateMedicineCommandValidator()
    {
        RuleFor(v => v.Name)
            .MaximumLength(200)
            .NotEmpty();

        RuleFor(v => v.DosageAmount)
            .GreaterThan(0)
            .WithMessage("Dosage amount must be greater than 0");

        RuleFor(v => v.Unit)
            .IsInEnum();

        RuleFor(v => v.UserEmail)
            .NotEmpty()
            .EmailAddress();
    }
}
