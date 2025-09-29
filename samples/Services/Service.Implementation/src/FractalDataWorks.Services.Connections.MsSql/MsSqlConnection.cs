using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using FractalDataWorks.Services.Connections.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace FractalDataWorks.Services.Connections.MsSql;

/// <summary>
/// SQL Server implementation of IGenericConnection.
/// </summary>
public sealed class MsSqlConnection : IGenericConnection
{
    private readonly ILogger<MsSqlConnection> _logger;
    private readonly MsSqlConfiguration _configuration;
    private SqlConnection? _sqlConnection;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlConnection"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="configuration">The configuration.</param>
    public MsSqlConnection(ILogger<MsSqlConnection> logger, MsSqlConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

        ConnectionId = Guid.NewGuid().ToString("N");
        _sqlConnection = new SqlConnection(_configuration.ConnectionString);
    }

    /// <inheritdoc />
    public string ConnectionId { get; }

    /// <inheritdoc />
    public string ProviderName => "MsSql";

    /// <inheritdoc />
    public async Task<IGenericResult> Execute(
        IDataCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await Execute<object>(command, cancellationToken);
        return result.IsSuccess ? GenericResult.Success() : GenericResult.Failure(result.Error);
    }

    /// <inheritdoc />
    public async Task<IGenericResult<TResult>> Execute<TResult>(
        IDataCommand command,
        CancellationToken cancellationToken = default)
    {
        if (_disposed)
            return GenericResult.Failure<TResult>("Connection has been disposed");

        if (command == null)
            return GenericResult.Failure<TResult>("Command cannot be null");

        try
        {
            if (_sqlConnection?.State != ConnectionState.Open)
            {
                await _sqlConnection!.OpenAsync(cancellationToken);
            }

            using var sqlCommand = new SqlCommand(BuildSqlCommand(command), _sqlConnection)
            {
                CommandTimeout = _configuration.CommandTimeout
            };

            AddParameters(sqlCommand, command);

            var result = await ExecuteCommand<TResult>(sqlCommand, command.CommandType, cancellationToken);
            return GenericResult.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute command on connection {ConnectionId}", ConnectionId);
            return GenericResult.Failure<TResult>(ex.Message);
        }
    }

    private string BuildSqlCommand(IDataCommand command)
    {
        // Basic SQL command building - in real implementation, use proper query builder
        return command.CommandType.ToUpperInvariant() switch
        {
            "QUERY" => $"SELECT * FROM [{command.EntityName}]",
            "INSERT" => $"INSERT INTO [{command.EntityName}] DEFAULT VALUES",
            "UPDATE" => $"UPDATE [{command.EntityName}] SET Id = Id",
            "DELETE" => $"DELETE FROM [{command.EntityName}] WHERE 1=0",
            _ => throw new NotSupportedException($"Command type '{command.CommandType}' not supported")
        };
    }

    private void AddParameters(SqlCommand sqlCommand, IDataCommand command)
    {
        foreach (var param in command.Parameters)
        {
            sqlCommand.Parameters.AddWithValue($"@{param.Key}", param.Value ?? DBNull.Value);
        }
    }

    private async Task<TResult> ExecuteCommand<TResult>(
        SqlCommand sqlCommand,
        string commandType,
        CancellationToken cancellationToken)
    {
        return commandType.ToUpperInvariant() switch
        {
            "QUERY" => (TResult)(object)await sqlCommand.ExecuteScalarAsync(cancellationToken),
            "INSERT" or "UPDATE" or "DELETE" => (TResult)(object)await sqlCommand.ExecuteNonQueryAsync(cancellationToken),
            _ => throw new NotSupportedException($"Command type '{commandType}' not supported")
        } ?? default!;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;

        _sqlConnection?.Dispose();
        _sqlConnection = null;
        _disposed = true;
    }
}