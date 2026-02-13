namespace OrionSoft.AppCore.Infrastructure.Data.DAOs.Common;

public interface IAuditableBaseDao {

    public DateTime? CreatedAt { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public string? UpdatedBy { get; set; }

}
