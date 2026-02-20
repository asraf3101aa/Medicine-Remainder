using FluentValidation;

namespace MedicineReminder.Application.Features.Medicines.Commands;

public class DeleteMedicineCommandValidator : AbstractValidator<DeleteMedicineCommand>
{
    public DeleteMedicineCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");
    }
}
