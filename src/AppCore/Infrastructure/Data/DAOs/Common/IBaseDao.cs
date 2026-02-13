namespace OrionSoft.AppCore.Infrastructure.Data.DAOs.Common;

public interface IBaseDao<T> : IAuditableBaseDao {

    public T? Id { get; set; }

    /// <summary>
    /// Indica si la entidad DAO es nueva (no ha sido persistida).
    /// </summary>
    public bool IsNew => Id is null || EqualityComparer<T>.Default.Equals(Id, default!);

}
