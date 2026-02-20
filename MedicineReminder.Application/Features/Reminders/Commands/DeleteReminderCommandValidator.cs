using FluentValidation;

namespace MedicineReminder.Application.Features.Reminders.Commands;

public class DeleteReminderCommandValidator : AbstractValidator<DeleteReminderCommand>
{
    public DeleteReminderCommandValidator()
    {
        RuleFor(v => v.Id)
            .GreaterThan(0).WithMessage("Id must be greater than 0.");
    }
}
