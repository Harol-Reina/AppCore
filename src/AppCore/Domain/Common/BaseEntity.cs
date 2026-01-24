using System.Text.Json.Serialization;

namespace AppCore.Domain.Common;

public abstract class BaseEntity<T> : AuditableEntity {
    protected BaseEntity() { }

    protected BaseEntity(T id) {
        Id = id;
    }

    [JsonPropertyOrder(-1)]
    public T? Id { get; set; }

    /// <summary>
    /// Indica si la entidad es nueva (no ha sido persistida en la base de datos).
    /// Una entidad es nueva si su ID es null o tiene el valor por defecto.
    /// </summary>
    [JsonIgnore]
    public bool IsNew => Id is null || EqualityComparer<T>.Default.Equals(Id, default!);

    /// <summary>
    /// Indica si la entidad ha sido persistida en la base de datos.
    /// </summary>
    [JsonIgnore]
    public bool IsPersisted => !IsNew;
}
