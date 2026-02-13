using System.Data;
using App.Application.Common;
using App.Infrastructure.Data.DAOs;
using OrionSoft.AppCore.Application.DTOs;
using OrionSoft.AppCore.Application.Interfaces;
using OrionSoft.AppCore.Domain.Interfaces;
using OrionSoft.AppCore.Domain.Entities.Integrators;
using OrionSoft.AppCore.Infrastructure.Extensions;
using Dapper;

namespace App.Infrastructure.Repositories;

public class HttpRequestRepository(IDbConnectionFactory connectionFactory,
                                   IMappingService<HttpAuditEntity, HttpAuditDao> toDao,
                                   IMappingService<HttpAuditDao, HttpAuditEntity> toEntity,
                                   ICurrentUserService currentUserService,
                                   AppSettings appSettings)
: IHttpRequestRepository {

    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
    private readonly IMappingService<HttpAuditEntity, HttpAuditDao> _toDao = toDao;
    private readonly IMappingService<HttpAuditDao, HttpAuditEntity> _toEntity = toEntity;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    private readonly string TableName = $"{appSettings.SchemaDB}.HttpAudit";

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
        var sql = $"SELECT * FROM {TableName} WHERE Id = @Id";
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
            (traceid, endpoint, headers, method, statuscode, elapsedmilliseconds, body, response, internalerror, createdat, createdby, updatedat, updatedby) 
            VALUES 
            (@TraceId, @Endpoint, @Headers::jsonb, @Method, @StatusCode, @ElapsedMilliseconds, @Body::jsonb, @Response::jsonb, @InternalError, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy)
            RETURNING id";

        var id = await db.QuerySingleAsync<int>(sql, dao);
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
            SET statuscode = @StatusCode, 
                elapsedmilliseconds = @ElapsedMilliseconds, 
                response = @Response::jsonb, 
                internalerror = @InternalError, 
                updatedat = @UpdatedAt, 
                updatedby = @UpdatedBy
            WHERE id = @Id";
            // Note: TraceId, Endpoint, Headers, Method, Body usually don't change in an update for audit, 
            // but we update the response/status fields.

        await db.ExecuteAsync(sql, dao);
        return _toEntity.Map(dao);
    }

    [DapperAot]
    public async Task<bool> DelAsync(int id) {
        using var db = await _connectionFactory.CreateConnectionAsync();
        var sql = $"DELETE FROM {TableName} WHERE Id = @Id";
        var affected = await db.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }

    [DapperAot]
    public Task<PaginationResponse<HttpAuditEntity>> GetPagedAsync(int page, int pageSize)
        => GetPagedAsync(page, pageSize, "Id", true); // Default sort

    [DapperAot]
    public async Task<PaginationResponse<HttpAuditEntity>> GetPagedAsync(int page, int pageSize, string sort, bool asc) {
         using var db = await _connectionFactory.CreateConnectionAsync();
        var offset = (page - 1) * pageSize;

        var allowedSortColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
            { "Id", "Id" },
            { "CreatedAt", "CreatedAt" },
            { "ElapsedMilliseconds", "ElapsedMilliseconds" }
        };

        var sortColumn = allowedSortColumns.GetValueOrDefault(sort, "Id");
        var direction = asc ? "ASC" : "DESC";

        var sql = $@"
            SELECT COUNT(*) FROM {TableName};
            SELECT * FROM {TableName} ORDER BY {sortColumn} {direction} LIMIT @PageSize OFFSET @Offset";

        using var multi = await db.QueryMultipleAsync(sql, new { PageSize = pageSize, Offset = offset });
        var totalItems = await multi.ReadFirstAsync<int>();
        var daos = await multi.ReadAsync<HttpAuditDao>();

        return new PaginationResponse<HttpAuditEntity> {
            Count = totalItems,
            Pages = (int)Math.Ceiling((double)totalItems / pageSize),
            Results = _toEntity.Map(daos.ToList()).ToList()
        };
    }
}
