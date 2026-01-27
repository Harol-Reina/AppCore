using AppCore.Infrastructure.Data.DAOs.Common;

namespace App.Infrastructure.Data.DAOs;

public class EmployeDao : BaseDao<int>, IAuditableBaseDao {
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
