using MedicineReminder.Domain.Common;

namespace MedicineReminder.Domain.Entities;

public class UserDevice : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = null!;

    public string? FcmToken { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}