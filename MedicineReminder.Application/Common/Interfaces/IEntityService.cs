using MedicineReminder.Application.Common.Models;

namespace MedicineReminder.Application.Common.Interfaces;

public interface IEntityService<T> where T : class
{
    Task<ServiceResult<T>> GetByIdAsync(string id);
    Task<ServiceResult<PaginatedList<T>>> GetAllAsync(PaginationQuery query);
    Task<ServiceResult<T>> CreateAsync(T entity);
    Task<ServiceResult> UpdateAsync(T entity);
    Task<ServiceResult> DeleteAsync(string id);
}