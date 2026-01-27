using System.Data;
using App.Application.Domain.Entities;
using App.Application.Common;
using App.Application.Interfaces;
using App.Infrastructure.Data.DAOs;
using AppCore.Application.DTOs;
using AppCore.Application.Interfaces;

using Dapper;

using AppCore.Infrastructure.Extensions;

namespace App.Infrastructure.Repositories;

public class EmployeRepository(IDbConnectionFactory connectionFactory, 
                               IMappingService<EmployeEntity, EmployeDao> toDao,
                               IMappingService<EmployeDao, EmployeEntity> toEntity,
                               ICurrentUserService currentUserService)
: IEmployeRepository {

    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
    private readonly IMappingService<EmployeEntity, EmployeDao> _toDao = toDao;
    private readonly IMappingService<EmployeDao, EmployeEntity> _toEntity = toEntity;
    private readonly ICurrentUserService _currentUserService = currentUserService;
    
    // Use AppConstants.SchemaDB to determine the schema
    private static readonly string TableName = $"{AppConstants.SchemaDB}.\"Employes\"";

    [DapperAot]
    public async Task<List<EmployeEntity>?> GetAllAsync() {
        using var db = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {TableName}";
        var daos = await db.QueryAsync<EmployeDao>(sql);
        return _toEntity.Map(daos.ToList()).ToList();
    }

    [DapperAot]
    public async Task<EmployeEntity?> GetByIdAsync(int id) {
        using var db = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {TableName} WHERE \"Id\" = @Id";
        var dao = await db.QueryFirstOrDefaultAsync<EmployeDao>(sql, new { Id = id });
        return dao != null ? _toEntity.Map(dao) : null;
    }

    [DapperAot]
    public async Task<EmployeEntity> AddAsync(EmployeEntity entity) {
        var dao = _toDao.Map(entity);
        dao.SetAuditCreate(_currentUserService);
        
        using var db = await _connectionFactory.CreateConnectionAsync();
        
        // Assuming Identity column for Id
        var sql = $@"
            INSERT INTO {TableName} (""Name"", ""Email"", ""Phone"", ""CreatedAt"", ""CreatedBy"", ""UpdatedAt"", ""UpdatedBy"") 
            VALUES (@Name, @Email, @Phone, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy)
            RETURNING ""Id""";
            
        var id = await db.ExecuteScalarAsync<int>(sql, dao);
        dao.Id = id;
        
        return _toEntity.Map(dao);
    }

    [DapperAot]
    public async Task<EmployeEntity> UpdateAsync(EmployeEntity entity) {
        var dao = _toDao.Map(entity);
        dao.SetAuditUpdate(_currentUserService);
        
        using var db = await _connectionFactory.CreateConnectionAsync();
        
        var sql = $@"
            UPDATE {TableName} 
            SET ""Name"" = @Name, ""Email"" = @Email, ""Phone"" = @Phone, ""UpdatedAt"" = @UpdatedAt, ""UpdatedBy"" = @UpdatedBy
            WHERE ""Id"" = @Id";
            
        await db.ExecuteAsync(sql, dao);
        return _toEntity.Map(dao);
    }

    [DapperAot]
    public async Task<bool> DelAsync(int id) {
        using var db = await _connectionFactory.CreateConnectionAsync();
        var sql = $"DELETE FROM {TableName} WHERE \"Id\" = @Id";
        var affected = await db.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    [DapperAot] // Mark overload for AOT analysis just in case
    public Task<PaginationDto<EmployeEntity>> GetPagedAsync(int page, int pageSize) 
        => GetPagedAsync(page, pageSize, "Id", true);

    [DapperAot]
    public async Task<PaginationDto<EmployeEntity>> GetPagedAsync(int page, int pageSize, string sort, bool asc) {
        using var db = await _connectionFactory.CreateConnectionAsync();
        var offset = (page - 1) * pageSize;

        // Whitelist for sorting to prevent SQL Injection
        var allowedSortColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            { "Id", "\"Id\"" },
            { "Name", "\"Name\"" },
            { "Email", "\"Email\"" },
            { "Phone", "\"Phone\"" },
            { "CreatedAt", "\"CreatedAt\"" }
        };

        var sortColumn = allowedSortColumns.GetValueOrDefault(sort, "\"Id\"");
        var direction = asc ? "ASC" : "DESC";
        
        var sql = $@"
            SELECT COUNT(*) FROM {TableName};
            SELECT * FROM {TableName} ORDER BY {sortColumn} {direction} LIMIT @PageSize OFFSET @Offset";
            
        using var multi = await db.QueryMultipleAsync(sql, new { PageSize = pageSize, Offset = offset });
        var totalItems = await multi.ReadFirstAsync<int>();
        var daos = await multi.ReadAsync<EmployeDao>();
        
        return new PaginationDto<EmployeEntity> {
            Count = totalItems,
            Pages = (int)Math.Ceiling((double)totalItems / pageSize),
            Results = _toEntity.Map(daos.ToList()).ToList()
        };
    }
}
