using MedicineReminder.Domain.Common;
using MedicineReminder.Domain.Enums;

namespace MedicineReminder.Domain.Entities;

public class Medicine : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public double DosageAmount { get; set; }
    public DosageUnit Unit { get; set; }
    public MedicineType Type { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string UserId { get; set; } = string.Empty;

    public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
}
