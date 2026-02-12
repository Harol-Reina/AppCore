using System.Net;
using System.Text.Json;
using AppCore.Domain.Common;

namespace AppCore.Domain.Entities.Integrators;

public sealed class HttpAuditEntity(Guid traceId,
                             string url,
                             HttpMethod? method = null,
                             JsonDocument? body = null,
                             JsonDocument? headers = null) : BaseEntity<int>() {

    public Guid TraceId { get; init; } = traceId;
    public string Endpoint { get; init; } = url;
    public JsonDocument? Headers { get; init; } = headers;
    public HttpMethod Method { get; init; } = method ?? HttpMethod.Get;
    public HttpStatusCode? StatusCode { get; set; }
    public long ElapsedMilliseconds { get; set; }
    public JsonDocument? Body { get; init; } = body;
    public JsonDocument? Response { get; set; }
    public string? InternalError { get; set; }
}
