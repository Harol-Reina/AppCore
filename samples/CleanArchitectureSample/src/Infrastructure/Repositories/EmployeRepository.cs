using App.Application.Domain.Entities;
using App.Application.Interfaces;
using App.Infrastructure.Data;
using App.Infrastructure.Data.DAOs;
using AppCore.Infrastructure.Repositories;
using AppCore.Application.Interfaces;

namespace App.Infrastructure.Repositories;

public class EmployeRepository(AppDbContext context, 
                               IMappingService<EmployeEntity, EmployeDao> toDao,
                               IMappingService<EmployeDao, EmployeEntity> toEntity)
: GenericRepository<EmployeEntity, int, EmployeDao>(context, toDao, toEntity), IEmployeRepository {
    
   
}
