using System.Data;

namespace OrionSoft.AppCore.Application.Interfaces;

/// <summary>
/// Factory interface for creating database connections.
/// </summary>
public interface IDbConnectionFactory {
    /// <summary>
    /// Creates a new database connection.
    /// </summary>
    /// <returns>A new IDbConnection instance.</returns>
    Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}
