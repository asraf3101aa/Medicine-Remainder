using Medicine.Domain.Common;
using Medicine.Domain.Enums;

namespace Medicine.Domain.Entities;

public class MedicineEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public double DosageAmount { get; set; }
    public DosageUnit Unit { get; set; }
    public string? Description { get; set; }
    public string UserEmail { get; set; } = string.Empty;

    public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
}
