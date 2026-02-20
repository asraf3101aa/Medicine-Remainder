using MedicineReminder.Application.Common.Mappings;
using MedicineReminder.Domain.Entities;

namespace MedicineReminder.Application.Features.Reminders.Queries;

public class ReminderDto : IMapFrom<Reminder>
{
    public string Id { get; set; }
    public string MedicineId { get; set; }
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
