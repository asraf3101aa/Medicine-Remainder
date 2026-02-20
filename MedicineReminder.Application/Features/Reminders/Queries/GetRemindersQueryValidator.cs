using FluentValidation;

namespace MedicineReminder.Application.Features.Reminders.Queries;

public class GetRemindersQueryValidator : AbstractValidator<GetRemindersQuery>
{
    public GetRemindersQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("PageNumber at least greater than or equal to 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("PageSize at least greater than or equal to 1.");
    }
}
