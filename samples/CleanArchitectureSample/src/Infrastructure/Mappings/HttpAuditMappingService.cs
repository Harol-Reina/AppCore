using System.Net;
using System.Text.Json;
using App.Infrastructure.Data.DAOs;
using AppCore.Application.Extensions;
using AppCore.Application.Interfaces;
using AppCore.Domain.Entities.Integrators;

namespace App.Infrastructure.Mappings;

public class HttpAuditMappingService : IMappingService<HttpAuditEntity, HttpAuditDao>, IMappingService<HttpAuditDao, HttpAuditEntity>
{
    public HttpAuditDao Map(HttpAuditEntity source)
    {
        return new HttpAuditDao
        {
            Id = source.Id,
            TraceId = source.TraceId,
            Endpoint = source.Endpoint,
            Headers = source.Headers != null ? JsonExtend.Serialize(source.Headers) : null,
            Method = source.Method.Method,
            StatusCode = source.StatusCode.HasValue ? (int)source.StatusCode.Value : null,
            ElapsedMilliseconds = source.ElapsedMilliseconds,
            Body = source.Body != null ? JsonExtend.Serialize(source.Body) : null,
            Response = source.Response != null ? JsonExtend.Serialize(source.Response) : null,
            InternalError = source.InternalError,
            CreatedAt = source.CreatedAt ?? DateTime.Now,
            CreatedBy = source.CreatedBy,
            UpdatedAt = source.UpdatedAt,
            UpdatedBy = source.UpdatedBy
        };
    }

    public HttpAuditEntity Map(HttpAuditDao source)
    {
        return new HttpAuditEntity(
            source.TraceId,
            source.Endpoint,
            new HttpMethod(source.Method),
            null, // Body is handled below via init
            null  // Headers handled below
        )
        {
            Id = source.Id,
            Headers = source.Headers != null ? JsonExtend.Deserialize<JsonDocument>(source.Headers) : null,
            StatusCode = source.StatusCode.HasValue ? (HttpStatusCode)source.StatusCode.Value : null,
            ElapsedMilliseconds = source.ElapsedMilliseconds,
            Body = source.Body != null ? JsonExtend.Deserialize<JsonDocument>(source.Body) : null,
            Response = source.Response != null ? JsonExtend.Deserialize<JsonDocument>(source.Response) : null,
            InternalError = source.InternalError,
            CreatedAt = source.CreatedAt,
            CreatedBy = source.CreatedBy,
            UpdatedAt = source.UpdatedAt,
            UpdatedBy = source.UpdatedBy
        };
    }

    public IEnumerable<HttpAuditDao> Map(IEnumerable<HttpAuditEntity> sources)
    {
        return sources.Select(Map);
    }

    public IEnumerable<HttpAuditEntity> Map(IEnumerable<HttpAuditDao> sources)
    {
        return sources.Select(Map);
    }
}
