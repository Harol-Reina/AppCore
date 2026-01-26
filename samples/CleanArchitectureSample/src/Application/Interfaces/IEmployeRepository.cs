using App.Application.Domain.Entities;
using AppCore.Domain.Interfaces;

namespace App.Application.Interfaces;

public interface IEmployeRepository : IGenericRepository<EmployeEntity, int> {}
