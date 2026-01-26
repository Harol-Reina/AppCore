using App.Application.Domain.Entities;
using App.Infrastructure.Data.DAOs;
using AppCore.Application.Interfaces;

namespace App.Infrastructure.Mappings;

public class EmployeMappingService : IMappingService<EmployeEntity, EmployeDao>, IMappingService<EmployeDao, EmployeEntity>
{
    public EmployeDao Map(EmployeEntity source)
    {
        return new EmployeDao
        {
            Id = source.Id, 
            Name = source.Name,
            Email = source.Email,
            Phone = source.Phone,
            CreatedAt = source.CreatedAt ?? DateTime.Now,
            CreatedBy = source.CreatedBy,
            UpdatedAt = source.UpdatedAt,
            UpdatedBy = source.UpdatedBy
        };
    }

    public EmployeEntity Map(EmployeDao source)
    {
        return new EmployeEntity
        {
            Id = source.Id,
            Name = source.Name,
            Email = source.Email,
            Phone = source.Phone,
            CreatedAt = source.CreatedAt,
            CreatedBy = source.CreatedBy,
            UpdatedAt = source.UpdatedAt,
            UpdatedBy = source.UpdatedBy
        };
    }

    public IEnumerable<EmployeDao> Map(IEnumerable<EmployeEntity> sources)
    {
        return sources.Select(Map);
    }

    public IEnumerable<EmployeEntity> Map(IEnumerable<EmployeDao> sources)
    {
        return sources.Select(Map);
    }
}
