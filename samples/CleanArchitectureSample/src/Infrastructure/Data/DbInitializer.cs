using App.Application.Common;
using AppCore.Application.Interfaces;
using Dapper;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Data;

public class DbInitializer(IDbConnectionFactory connectionFactory, ILogger<DbInitializer> logger) {
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;
    private readonly ILogger<DbInitializer> _logger = logger;

    public async Task InitAsync() {
        try {
            using var connection = await _connectionFactory.CreateConnectionAsync();
            var schema = AppConstants.SchemaDB;

            _logger.LogInformation("Initializing database schema...");

            var sql = $@"
                CREATE SCHEMA IF NOT EXISTS {schema};


                CREATE TABLE IF NOT EXISTS {schema}.Employes (
                    Id SERIAL PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    Phone TEXT NOT NULL,
                    CreatedAt TIMESTAMP NOT NULL,
                    CreatedBy TEXT,
                    UpdatedAt TIMESTAMP,
                    UpdatedBy TEXT
                );

                CREATE TABLE IF NOT EXISTS {schema}.HttpAudit (
                    Id SERIAL PRIMARY KEY,
                    TraceId UUID NOT NULL,
                    Endpoint TEXT NOT NULL,
                    Headers JSONB,
                    Method TEXT NOT NULL,
                    StatusCode INTEGER,
                    ElapsedMilliseconds BIGINT NOT NULL,
                    Body JSONB,
                    Response JSONB,
                    InternalError TEXT,
                    CreatedAt TIMESTAMP NOT NULL,
                    CreatedBy TEXT,
                    UpdatedAt TIMESTAMP,
                    UpdatedBy TEXT
                );
            ";

            await connection.ExecuteAsync(sql);
            _logger.LogInformation("Database schema initialized successfully.");
        } catch (Exception ex) {
            _logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }
}
