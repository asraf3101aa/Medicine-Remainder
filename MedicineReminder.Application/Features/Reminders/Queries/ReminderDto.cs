using MedicineReminder.Application.Common.Mappings;
using MedicineReminder.Domain.Entities;

namespace MedicineReminder.Application.Features.Reminders.Queries;

public class ReminderDto : IMapFrom<Reminder>
{
    public int Id { get; set; }
    public int MedicineId { get; set; }
    public string MedicineName { get; set; } = string.Empty;
    public DateTime ReminderUtc { get; set; }
    public bool IsTaken { get; set; }
    public bool IsActive { get; set; }

    public void Mapping(AutoMapper.Profile profile)
    {
        profile.CreateMap<Reminder, ReminderDto>()
            .ForMember(d => d.MedicineName, opt => opt.MapFrom(s => s.Medicine.Name));
    }
}
