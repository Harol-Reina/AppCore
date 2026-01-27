namespace AppCore.Infrastructure.Data.DAOs.Common;

public abstract class BaseDao<T>  {

    public T? Id { get; set; }

    /// <summary>
    /// Indica si la entidad DAO es nueva (no ha sido persistida).
    /// </summary>
    public bool IsNew => Id is null || EqualityComparer<T>.Default.Equals(Id, default!);

}
