using System.Text.Json;
using AppCore.Infrastructure.Data.DAOs.Common;

namespace App.Infrastructure.Data.DAOs;

public class HttpAuditDao : IBaseDao<int> {
    public int Id { get; set; }
    public Guid TraceId { get; set; }
    public string Endpoint { get; set; } = null!;
    public JsonDocument? Headers { get; set; }
    public string Method { get; set; } = null!;
    public int? StatusCode { get; set; }
    public long ElapsedMilliseconds { get; set; }
    public JsonDocument? Body { get; set; }
    public JsonDocument? Response { get; set; }
    public string? InternalError { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
