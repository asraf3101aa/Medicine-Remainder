using AutoMapper;
using Medicine.Application.Features.Medicines.Queries;
using Medicine.Domain.Entities;

namespace Medicine.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<MedicineEntity, MedicineDto>();
    }
}
