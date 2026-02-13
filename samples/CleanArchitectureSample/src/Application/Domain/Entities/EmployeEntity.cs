using OrionSoft.AppCore.Application.Interfaces;
using OrionSoft.AppCore.Domain.Common;

namespace App.Application.Domain.Entities;

public class EmployeEntity : BaseEntity<int> {
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Phone { get; set; } = null!;
}
