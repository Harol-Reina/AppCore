using App.Application.Domain.Entities;
using App.Application.DTOs.Request;
using App.Application.DTOs.Response;

namespace App.Application.Common.Mappings;

public static class EmployeMappings
{
    public static EmployeEntity ToEntity(this EmployeRequestDto request)
    {
        return new EmployeEntity
        {
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone
        };
    }

    public static EmployeResponseDto ToDto(this EmployeEntity entity)
    {
        return new EmployeResponseDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Email = entity.Email,
            Phone = entity.Phone
        };
    }

    public static List<EmployeResponseDto> ToDto(this IEnumerable<EmployeEntity> entities)
    {
        return entities.Select(x => x.ToDto()).ToList();
    }
}
