using MedicineReminder.Application.Common.Mappings;
using MedicineReminder.Domain.Enums;

namespace MedicineReminder.Application.Features.Medicines.Queries;

public class MedicineDto : IMapFrom<MedicineReminder.Domain.Entities.Medicine>
{
    public string Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double DosageAmount { get; set; }
    public DosageUnit Unit { get; set; }
    public MedicineType Type { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}
