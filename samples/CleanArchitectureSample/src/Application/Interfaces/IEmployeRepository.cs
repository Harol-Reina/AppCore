using App.Application.Domain.Entities;
using AppCore.Application.DTOs;
using AppCore.Domain.Interfaces;

namespace App.Application.Interfaces;

public interface IEmployeRepository : IGenericRepository<EmployeEntity, int> {
    Task<PaginationDto<EmployeEntity>> GetPagedAsync(int page, int pageSize, string sort, bool asc);
}
