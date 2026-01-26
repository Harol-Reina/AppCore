using System.Data;
using AppCore.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace AppCore.Infrastructure.Data;

/// <summary>
/// PostgreSQL implementation of IDbConnectionFactory using Npgsql.
/// </summary>
public sealed class NpgsqlConnectionFactory(IConfiguration configuration) : IDbConnectionFactory {
    private readonly string _connectionString = configuration.GetConnectionString("DefaultConnection") 
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default) {
        var connection = new NpgsqlDataSourceBuilder(_connectionString).Build().CreateConnection();
        // Note: OpenAsync is not strictly required by IDbConnection interface which is synchronous regarding Open(),
        // but for async contexts we might want to ensure it's valid. However, Dapper usually handles opening.
        // We will just return the connection.
        // If we want to support async opening, we might need to return a Task<IDbConnection> which we do.
        
        // Wait, NpgsqlConnection.OpenAsync is explicit.
        // But IDbConnection.Open is void.
        // We should return the connection and let the consumer open it, OR open it here async.
        // Opening here is safer for the "Async" factory contract.
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
