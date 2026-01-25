using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppCore.Infrastructure.Data.DAOs.Common;

internal abstract class BaseDao<T> : AuditableBaseDao {

    [Key]
    public T? Id { get; set; }

    /// <summary>
    /// Indica si la entidad DAO es nueva (no ha sido persistida).
    /// </summary>
    public bool IsNew => Id is null || EqualityComparer<T>.Default.Equals(Id, default!);

}

internal abstract class BaseDaoInt : BaseDao<int> {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public new int Id { get; set; }
}
