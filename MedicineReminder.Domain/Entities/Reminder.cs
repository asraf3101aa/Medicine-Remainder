using MedicineReminder.Domain.Common;

namespace MedicineReminder.Domain.Entities;

public class Reminder : BaseEntity
{
    public string MedicineId { get; set; } = string.Empty;
    public Medicine Medicine { get; set; } = null!;
    public DateTime ReminderUtc { get; set; }
    public bool IsTaken { get; set; }
    public DateTime? TakenAtUtc { get; set; }
    public int SnoozeCount { get; set; }
    public int SnoozeDurationMinutes { get; set; } = 5;
    public DateTime NextReminderUtc { get; set; }
    public bool IsActive { get; set; } = true;
}
