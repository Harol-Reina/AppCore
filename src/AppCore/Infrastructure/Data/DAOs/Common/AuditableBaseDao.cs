using System.ComponentModel.DataAnnotations.Schema;

namespace AppCore.Infrastructure.Data.DAOs.Common;

public abstract class AuditableBaseDao {

    [Column(TypeName = "timestamp without time zone")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string? CreatedBy { get; set; } = null!;

    [Column(TypeName = "timestamp without time zone")]
    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

}
