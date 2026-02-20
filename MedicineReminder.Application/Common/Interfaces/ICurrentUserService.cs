namespace MedicineReminder.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserEmail { get; }
    string? UserId { get; }
}
