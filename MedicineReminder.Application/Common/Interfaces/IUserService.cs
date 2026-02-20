using MedicineReminder.Application.Common.Models;
using MedicineReminder.Domain.Entities;

namespace MedicineReminder.Application.Common.Interfaces;

public interface IUserService : IEntityService<User>
{
    Task<ServiceResult<User>> GetByEmailAsync(string email);
}