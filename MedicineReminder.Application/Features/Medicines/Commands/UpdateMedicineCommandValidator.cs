using FluentValidation;

namespace MedicineReminder.Application.Features.Medicines.Commands;

public class UpdateMedicineCommandValidator : AbstractValidator<UpdateMedicineCommand>
{
    public UpdateMedicineCommandValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(v => v.DosageAmount)
            .NotEmpty().WithMessage("Dosage amount must be greater than 0.");

        RuleFor(v => v.Unit)
            .IsInEnum().WithMessage("Invalid dosage unit.");

        RuleFor(v => v.Type)
            .IsInEnum().WithMessage("Invalid medicine type.");

        RuleFor(v => v.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(v => v.EndDate)
            .GreaterThan(v => v.StartDate).When(v => v.EndDate.HasValue)
            .WithMessage("End date must be after start date.");
    }
}
