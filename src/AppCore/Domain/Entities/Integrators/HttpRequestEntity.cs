using System.Net;
using System.Text.Json;
using AppCore.Application.Extensions;
using AppCore.Domain.Common;

namespace AppCore.Domain.Entities.Integrators;

public class HttpAuditEntity(Guid traceId,
                             string url,
                             HttpMethod? method = null,
                             object? body = null,
                             Dictionary<string, string>? headers = null) : BaseEntity<int>() {

    public Guid TraceId { get; init; } = traceId;
    public string Endpoint { get; init; } = url;
    public JsonDocument? Headers { get; init; } = JsonExtend.ToJsonDocument(headers);
    public HttpMethod Method { get; init; } = method ?? HttpMethod.Get;
    public HttpStatusCode? StatusCode { get; set; }
    public long ElapsedMilliseconds { get; set; }
    public JsonDocument? Body { get; init; } = JsonExtend.ToJsonDocument(body);
    public JsonDocument? Response { get; set; }
    public string? InternalError { get; set; }
}
