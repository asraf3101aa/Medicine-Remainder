using FluentValidation;

namespace MedicineReminder.Application.Features.Medicines.Commands;

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

        RuleFor(v => v.Type)
            .IsInEnum();

        RuleFor(v => v.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(v => v.EndDate)
            .Must((cmd, endDate) => !endDate.HasValue || endDate.Value >= cmd.StartDate)
            .WithMessage("End date must be after start date.");
    }
}
