using System.Text.Json.Serialization;

namespace AppCore.Domain.Common;

/// <summary>
/// Base class for domain entities providing common properties and behavior.
/// Inherits auditing capabilities and provides identity management.
/// </summary>
/// <typeparam name="T">The type of the entity's unique identifier.</typeparam>
public abstract class BaseEntity<T> : AuditableEntity {
    /// <summary>
    /// Initializes a new instance of the BaseEntity class with default values.
    /// </summary>
    protected BaseEntity() { }

    /// <summary>
    /// Initializes a new instance of the BaseEntity class with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the entity.</param>
    protected BaseEntity(T id) {
        Id = id;
    }

    /// <summary>
    /// Gets or sets the unique identifier of the entity.
    /// </summary>
    /// <value>The entity's unique identifier.</value>
    [JsonPropertyOrder(-1)]
    public T? Id { get; set; }

    /// <summary>
    /// Indica si la entidad es nueva (no ha sido persistida en la base de datos).
    /// Una entidad es nueva si su ID es null o tiene el valor por defecto.
    /// </summary>
    /// <value>True if the entity has not been persisted; otherwise, false.</value>
    [JsonIgnore]
    public bool IsNew => Id is null || EqualityComparer<T>.Default.Equals(Id, default!);

    /// <summary>
    /// Indica si la entidad ha sido persistida en la base de datos.
    /// </summary>
    /// <value>True if the entity has been persisted; otherwise, false.</value>
    [JsonIgnore]
    public bool IsPersisted => !IsNew;
}
