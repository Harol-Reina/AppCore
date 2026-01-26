using System.ComponentModel.DataAnnotations.Schema;
using AppCore.Infrastructure.Data.DAOs.Common;

namespace App.Infrastructure.Data.DAOs;

[Table("Employe")]
public class EmployeDao : BaseDao<int> {
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
}
