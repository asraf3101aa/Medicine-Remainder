using MedicineReminder.Application.Common.Models;
using MedicineReminder.Domain.Entities;
using MedicineReminder.Domain.Enums;

namespace MedicineReminder.Application.Common.Interfaces;

public interface IMedicineService : IEntityService<Medicine>
{
    Task<ServiceResult<PaginatedList<Medicine>>> GetUserMedicinesAsync(string userId, PaginationQuery query);
    Task<ServiceResult<PaginatedList<Medicine>>> GetActiveMedicinesAsync(string userId, PaginationQuery query);
    Task<ServiceResult<PaginatedList<Medicine>>> GetMedicinesByTypeAsync(string userId, MedicineType type, PaginationQuery query);
    Task<ServiceResult<int>> GetUserMedicinesCountAsync(string userId);
}