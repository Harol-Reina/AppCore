using System.Text.Json.Serialization;

namespace AppCore.Domain.Common;

public abstract class AuditableEntity
{

    [JsonPropertyOrder(100)]
    public string? CreatedBy { get; set; }

    [JsonPropertyOrder(101)]
    public DateTime? CreatedAt { get; set; }

    [JsonPropertyOrder(102)]
    public string? UpdatedBy { get; set; }

    [JsonPropertyOrder(103)]
    public DateTime? UpdatedAt { get; set; }
}
