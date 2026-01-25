using System.Text.Json.Serialization;

namespace AppCore.Domain.Common;

/// <summary>
/// Abstract base class that provides audit trail properties for entities.
/// Tracks creation and modification information including user and timestamp.
/// </summary>
public abstract class AuditableEntity
{
    /// <summary>
    /// Gets or sets the identifier of the user who created this entity.
    /// </summary>
    /// <value>The user identifier or username who created the entity.</value>
    [JsonPropertyOrder(100)]
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entity was created.
    /// </summary>
    /// <value>The creation timestamp.</value>
    [JsonPropertyOrder(101)]
    public DateTime? CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last updated this entity.
    /// </summary>
    /// <value>The user identifier or username who last modified the entity.</value>
    [JsonPropertyOrder(102)]
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when this entity was last updated.
    /// </summary>
    /// <value>The last modification timestamp.</value>
    [JsonPropertyOrder(103)]
    public DateTime? UpdatedAt { get; set; }
}
