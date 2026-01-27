using System.Data;
using App.Application.Common;
using App.Infrastructure.Data.DAOs;
using AppCore.Application.DTOs;
using AppCore.Application.Interfaces;
using AppCore.Domain.Interfaces;
using AppCore.Domain.Entities.Integrators;
using AppCore.Infrastructure.Extensions;
using Dapper;

namespace App.Infrastructure.Repositories;

public class HttpRequestRepository(IDbConnectionFactory connectionFactory,
                                   IMappingService<HttpAuditEntity, HttpAuditDao> toDao,
                                   IMappingService<HttpAuditDao, HttpAuditEntity> toEntity,
                                   ICurrentUserService currentUserService)
: IHttpRequestRepository {

    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
    private readonly IMappingService<HttpAuditEntity, HttpAuditDao> _toDao = toDao;
    private readonly IMappingService<HttpAuditDao, HttpAuditEntity> _toEntity = toEntity;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    private static readonly string TableName = $"{AppConstants.SchemaDB}.\"HttpAudit\"";

    [DapperAot]
    public async Task<List<HttpAuditEntity>?> GetAllAsync() {
        using var db = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {TableName}";
        var daos = await db.QueryAsync<HttpAuditDao>(sql);
        return _toEntity.Map(daos.ToList()).ToList();
    }

    [DapperAot]
    public async Task<HttpAuditEntity?> GetByIdAsync(int id) {
        using var db = await _connectionFactory.CreateConnectionAsync();
        var sql = $"SELECT * FROM {TableName} WHERE \"Id\" = @Id";
        var dao = await db.QueryFirstOrDefaultAsync<HttpAuditDao>(sql, new { Id = id });
        return dao != null ? _toEntity.Map(dao) : null;
    }

    [DapperAot]
    public async Task<HttpAuditEntity> AddAsync(HttpAuditEntity entity) {
        var dao = _toDao.Map(entity);
        dao.SetAuditCreate(_currentUserService);

        using var db = await _connectionFactory.CreateConnectionAsync();

        var sql = $@"
            INSERT INTO {TableName} 
            (""TraceId"", ""Endpoint"", ""Headers"", ""Method"", ""StatusCode"", ""ElapsedMilliseconds"", ""Body"", ""Response"", ""InternalError"", ""CreatedAt"", ""CreatedBy"", ""UpdatedAt"", ""UpdatedBy"") 
            VALUES 
            (@TraceId, @Endpoint, @Headers::jsonb, @Method, @StatusCode, @ElapsedMilliseconds, @Body::jsonb, @Response::jsonb, @InternalError, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy)
            RETURNING ""Id""";

        var id = await db.ExecuteScalarAsync<int>(sql, dao);
        dao.Id = id;

        return _toEntity.Map(dao);
    }

    [DapperAot]
    public async Task<HttpAuditEntity> UpdateAsync(HttpAuditEntity entity) {
        var dao = _toDao.Map(entity);
        dao.SetAuditUpdate(_currentUserService);

        using var db = await _connectionFactory.CreateConnectionAsync();

        var sql = $@"
            UPDATE {TableName} 
            SET ""StatusCode"" = @StatusCode, 
                ""ElapsedMilliseconds"" = @ElapsedMilliseconds, 
                ""Response"" = @Response::jsonb, 
                ""InternalError"" = @InternalError, 
                ""UpdatedAt"" = @UpdatedAt, 
                ""UpdatedBy"" = @UpdatedBy
            WHERE ""Id"" = @Id";
            // Note: TraceId, Endpoint, Headers, Method, Body usually don't change in an update for audit, 
            // but we update the response/status fields.

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

    [DapperAot]
    public Task<PaginationDto<HttpAuditEntity>> GetPagedAsync(int page, int pageSize)
        => GetPagedAsync(page, pageSize, "Id", true); // Default sort

    [DapperAot]
    public async Task<PaginationDto<HttpAuditEntity>> GetPagedAsync(int page, int pageSize, string sort, bool asc) {
         using var db = await _connectionFactory.CreateConnectionAsync();
        var offset = (page - 1) * pageSize;

        var allowedSortColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            { "Id", "\"Id\"" },
            { "CreatedAt", "\"CreatedAt\"" },
            { "ElapsedMilliseconds", "\"ElapsedMilliseconds\"" }
        };

        var sortColumn = allowedSortColumns.GetValueOrDefault(sort, "\"Id\"");
        var direction = asc ? "ASC" : "DESC";

        var sql = $@"
            SELECT COUNT(*) FROM {TableName};
            SELECT * FROM {TableName} ORDER BY {sortColumn} {direction} LIMIT @PageSize OFFSET @Offset";

        using var multi = await db.QueryMultipleAsync(sql, new { PageSize = pageSize, Offset = offset });
        var totalItems = await multi.ReadFirstAsync<int>();
        var daos = await multi.ReadAsync<HttpAuditDao>();

        return new PaginationDto<HttpAuditEntity> {
            Count = totalItems,
            Pages = (int)Math.Ceiling((double)totalItems / pageSize),
            Results = _toEntity.Map(daos.ToList()).ToList()
        };
    }
}
