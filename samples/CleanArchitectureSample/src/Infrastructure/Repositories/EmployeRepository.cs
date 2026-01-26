using System.Data;
using App.Application.Domain.Entities;
using App.Application.Common;
using App.Application.Interfaces;
using App.Infrastructure.Data.DAOs;
using AppCore.Application.DTOs;
using AppCore.Application.Interfaces;

using Dapper;

namespace App.Infrastructure.Repositories;

public class EmployeRepository(IDbConnectionFactory connectionFactory, 
                               IMappingService<EmployeEntity, EmployeDao> toDao,
                               IMappingService<EmployeDao, EmployeEntity> toEntity)
: IEmployeRepository {

    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
    private readonly IMappingService<EmployeEntity, EmployeDao> _toDao = toDao;
    private readonly IMappingService<EmployeDao, EmployeEntity> _toEntity = toEntity;
    
    // Use AppConstants.SchemaDB to determine the schema
    private static readonly string TableName = $"{AppConstants.SchemaDB}.Employes";

    public async Task<List<EmployeEntity>?> GetAllAsync() {
        using var db = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {TableName}";
        var daos = await db.QueryAsync<EmployeDao>(sql);
        return _toEntity.Map(daos.ToList()).ToList();
    }

    public async Task<EmployeEntity?> GetByIdAsync(int id) {
        using var db = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {TableName} WHERE Id = @Id";
        var dao = await db.QueryFirstOrDefaultAsync<EmployeDao>(sql, new { Id = id });
        return dao != null ? _toEntity.Map(dao) : null;
    }

    public async Task<EmployeEntity> AddAsync(EmployeEntity entity) {
        var dao = _toDao.Map(entity);
        dao.CreatedAt = DateTime.UtcNow; // Manual auditing
        
        using var db = await _connectionFactory.CreateConnectionAsync();
        
        // Assuming Identity column for Id
        var sql = $@"
            INSERT INTO {TableName} (Name, Email, Phone, CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) 
            VALUES (@Name, @Email, @Phone, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy)
            RETURNING Id";
            
        var id = await db.ExecuteScalarAsync<int>(sql, dao);
        dao.Id = id;
        
        return _toEntity.Map(dao);
    }

    public async Task<EmployeEntity> UpdateAsync(EmployeEntity entity) {
        var dao = _toDao.Map(entity);
        dao.UpdatedAt = DateTime.UtcNow; // Manual auditing
        
        using var db = await _connectionFactory.CreateConnectionAsync();
        
        var sql = $@"
            UPDATE {TableName} 
            SET Name = @Name, Email = @Email, Phone = @Phone, UpdatedAt = @UpdatedAt, UpdatedBy = @UpdatedBy
            WHERE Id = @Id";
            
        await db.ExecuteAsync(sql, dao);
        return _toEntity.Map(dao);
    }

    public async Task<bool> DelAsync(int id) {
        using var db = await _connectionFactory.CreateConnectionAsync();
        var sql = $"DELETE FROM {TableName} WHERE Id = @Id";
        var affected = await db.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    public async Task<PaginationDto<EmployeEntity>> GetPagedAsync(int page, int pageSize) {
        using var db = await _connectionFactory.CreateConnectionAsync();
        var offset = (page - 1) * pageSize;
        
        var sql = $@"
            SELECT COUNT(*) FROM {TableName};
            SELECT * FROM {TableName} ORDER BY Id LIMIT @PageSize OFFSET @Offset";
            
        using var multi = await db.QueryMultipleAsync(sql, new { PageSize = pageSize, Offset = offset });
        var totalItems = await multi.ReadFirstAsync<int>();
        var daos = await multi.ReadAsync<EmployeDao>();
        
        return new PaginationDto<EmployeEntity> {
            Count = totalItems,
            Pages = (int)Math.Ceiling((double)totalItems / pageSize),
            Results = _toEntity.Map(daos.ToList()).OrderBy(r => r.Id).ToList()
        };
    }
}
