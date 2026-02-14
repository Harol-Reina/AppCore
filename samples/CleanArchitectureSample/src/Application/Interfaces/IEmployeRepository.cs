using App.Application.Domain.Entities;
using OrionSoft.AppCore.Domain.Common;
using OrionSoft.AppCore.Domain.Interfaces;

namespace App.Application.Interfaces;

public interface IEmployeRepository : IGenericRepository<EmployeEntity, int> {
    Task<PaginationResponse<EmployeEntity>> GetPagedAsync(int page, int pageSize, string sort, bool asc);
}
