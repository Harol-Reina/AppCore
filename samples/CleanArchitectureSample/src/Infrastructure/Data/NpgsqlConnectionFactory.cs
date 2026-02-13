using System.Data;
using App.Application.Common;
using OrionSoft.AppCore.Application.Interfaces;
using Npgsql;

namespace App.Infrastructure.Data;

/// <summary>
/// PostgreSQL implementation of IDbConnectionFactory using Npgsql.
/// </summary>
public sealed class NpgsqlConnectionFactory(AppSettings appSettings) : IDbConnectionFactory {
    private readonly string _connectionString = appSettings.DefaultConnection;

    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default) {
        var connection = new NpgsqlDataSourceBuilder(_connectionString).Build().CreateConnection();
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
