using Medicine.Domain.Enums;

namespace Medicine.Application.Features.Medicines.Queries;

public class MedicineDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double DosageAmount { get; set; }
    public DosageUnit Unit { get; set; }
    public string? Description { get; set; }
}
