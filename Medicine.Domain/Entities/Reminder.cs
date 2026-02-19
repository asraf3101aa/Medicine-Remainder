using Medicine.Domain.Common;

namespace Medicine.Domain.Entities;

public class Reminder : BaseEntity
{
    public int MedicineId { get; set; }
    public MedicineEntity Medicine { get; set; } = null!;
    public DateTime ReminderUtc { get; set; }
    public bool IsTaken { get; set; }
    public DateTime? TakenAtUtc { get; set; }
}
