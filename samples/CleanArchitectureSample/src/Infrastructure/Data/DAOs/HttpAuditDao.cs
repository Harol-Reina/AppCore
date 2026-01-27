using System.ComponentModel.DataAnnotations.Schema;
using AppCore.Infrastructure.Data.DAOs.Common;

namespace App.Infrastructure.Data.DAOs;

[Table("HttpAudit")]
public class HttpAuditDao : BaseDaoInt {
    public Guid TraceId { get; set; }
    public string Endpoint { get; set; } = null!;
    [Column(TypeName = "jsonb")]
    public string? Headers { get; set; }
    public string Method { get; set; } = null!;
    public int? StatusCode { get; set; }
    public long ElapsedMilliseconds { get; set; }
    [Column(TypeName = "jsonb")]
    public string? Body { get; set; }
    [Column(TypeName = "jsonb")]
    public string? Response { get; set; }
    public string? InternalError { get; set; }
}
