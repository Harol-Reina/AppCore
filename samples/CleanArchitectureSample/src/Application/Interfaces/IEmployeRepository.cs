using App.Application.Domain.Entities;
using OrionSoft.AppCore.Application.DTOs;
using OrionSoft.AppCore.Domain.Interfaces;

namespace App.Application.Interfaces;

public interface IEmployeRepository : IGenericRepository<EmployeEntity, int> {
    Task<PaginationResponse<EmployeEntity>> GetPagedAsync(int page, int pageSize, string sort, bool asc);
}
