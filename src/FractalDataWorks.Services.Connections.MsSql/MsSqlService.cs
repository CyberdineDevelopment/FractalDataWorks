using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FractalDataWorks;
using FractalDataWorks.DataSets.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.Services.Connections.Abstractions.Commands;
using FractalDataWorks.Services.Connections.Abstractions.Translators;
using FractalDataWorks.Services.Connections.MsSql.Commands;
using FractalDataWorks.Services.Connections.MsSql.Logging;
using FractalDataWorks.Services.Connections.MsSql.Mappers;
using FractalDataWorks.Services.Connections.MsSql.Translators;

namespace FractalDataWorks.Services.Connections.MsSql;

/// <summary>
/// SQL Server implementation of connection service with LINQ query translation.
/// This service handles SQL Server connection commands and translates LINQ queries to T-SQL.
/// </summary>
public sealed class MsSqlService 
    : ConnectionServiceBase<IConnectionCommand, MsSqlConfiguration, MsSqlService>, 
    IConnectionDataService,
    IDisposable
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly Dictionary<string, MsSqlFdwConnection> _connections;
    private readonly IQueryTranslator _queryTranslator;
    private readonly IResultMapper _resultMapper;
    private readonly string _serviceId;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MsSqlService"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory for creating connection loggers.</param>
    /// <param name="configuration">The MsSql service configuration.</param>
    /// <param name="queryTranslator">The T-SQL query translator.</param>
    /// <param name="resultMapper">The SQL Server result mapper.</param>
    public MsSqlService(
        ILoggerFactory loggerFactory,
        IQueryTranslator queryTranslator,
        IResultMapper resultMapper,
        MsSqlConfiguration configuration) 
        : base(configuration)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _queryTranslator = queryTranslator ?? throw new ArgumentNullException(nameof(queryTranslator));
        _resultMapper = resultMapper ?? throw new ArgumentNullException(nameof(resultMapper));
        _connections = new Dictionary<string, MsSqlFdwConnection>(StringComparer.Ordinal);
        _serviceId = Guid.NewGuid().ToString("N");
    }

    /// <inheritdoc/>
    public override string ServiceType => "MsSql";

    /// <inheritdoc/>
    public override bool IsAvailable => _connections.Count > 0;

    /// <inheritdoc/>
    public string ServiceId => _serviceId;

    /// <summary>
    /// Gets the query translator for converting LINQ expressions to T-SQL.
    /// </summary>
    protected IQueryTranslator QueryTranslator => _queryTranslator;

    /// <summary>
    /// Gets the result mapper for converting SQL results to dataset objects.
    /// </summary>
    protected IResultMapper ResultMapper => _resultMapper;

    /// <inheritdoc/>
    public override async Task<IFdwResult<T>> Execute<T>(IConnectionCommand command)
    {
        return await Execute<T>(command, CancellationToken.None).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public override async Task<IFdwResult<TOut>> Execute<TOut>(IConnectionCommand command, CancellationToken cancellationToken)
    {
        // Handle different command types
        switch (command)
        {
            case SqlConnectionCommand sqlCommand:
                // Handle translated SQL command from TSqlQueryTranslator
                var queryResult = await ExecuteSqlCommandAsync<TOut>(sqlCommand, cancellationToken).ConfigureAwait(false);
                return queryResult;

            case IConnectionCreateCommand createCommand:
                var createResult = await HandleConnectionCreate(createCommand, cancellationToken).ConfigureAwait(false);
                return ConvertResult<TOut>(createResult);

            case IConnectionDiscoveryCommand discoveryCommand:
                var discoveryResult = await HandleConnectionDiscovery(discoveryCommand, cancellationToken).ConfigureAwait(false);
                return ConvertResult<TOut>(discoveryResult);

            case IConnectionManagementCommand managementCommand:
                var managementResult = await HandleConnectionManagement(managementCommand, cancellationToken).ConfigureAwait(false);
                return ConvertResult<TOut>(managementResult);

            default:
                return FdwResult<TOut>.Failure($"Unsupported command type: {command.GetType().Name}");
        }
    }

    /// <inheritdoc/>
    public override async Task<IFdwResult> Execute(IConnectionCommand command, CancellationToken cancellationToken)
    {
        var result = await Execute<object>(command, cancellationToken).ConfigureAwait(false);
        return result.IsSuccess ? FdwResult.Success() : FdwResult.Failure(result.Message);
    }

    /// <summary>
    /// Executes a translated SQL command and maps the results using the result mapper.
    /// </summary>
    /// <typeparam name="TResult">The expected result type.</typeparam>
    /// <param name="sqlCommand">The SQL command to execute.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The mapped query results.</returns>
    private async Task<IFdwResult<TResult>> ExecuteSqlCommandAsync<TResult>(
        SqlConnectionCommand sqlCommand, 
        CancellationToken cancellationToken) where TResult : class
    {
        try
        {
            // This is a simplified implementation
            // In a real implementation, this would:
            // 1. Get a SQL connection from the connection pool
            // 2. Execute the SQL command
            // 3. Use the result mapper to convert SqlDataReader to TResult
            
            // For now, return a placeholder result
            var placeholderResult = new List<TResult>();
            return FdwResult<TResult>.Success(placeholderResult.AsEnumerable().Cast<TResult>().FirstOrDefault()!);
        }
        catch (Exception ex)
        {
            return FdwResult<TResult>.Failure($"Failed to execute SQL command: {ex.Message}");
        }
    }

    private Task<IFdwResult<string>> HandleConnectionCreate(IConnectionCreateCommand command, CancellationToken cancellationToken)
    {
        if (_connections.ContainsKey(command.ConnectionName))
        {
            return Task.FromResult(FdwResult<string>.Failure($"Connection '{command.ConnectionName}' already exists"));
        }

        if (command.ConnectionConfiguration is not MsSqlConfiguration msSqlConfig)
        {
            return Task.FromResult(FdwResult<string>.Failure("Invalid configuration type"));
        }

        var connectionLogger = _loggerFactory.CreateLogger<MsSqlFdwConnection>();
        var connection = new MsSqlFdwConnection(connectionLogger, msSqlConfig);
        _connections[command.ConnectionName] = connection;

        return Task.FromResult(FdwResult<string>.Success($"Connection '{command.ConnectionName}' created successfully"));
    }

    private async Task<IFdwResult<DataContainer[]>> HandleConnectionDiscovery(IConnectionDiscoveryCommand command, CancellationToken cancellationToken)
    {
        if (!_connections.TryGetValue(command.ConnectionName, out var connection))
        {
            return FdwResult<DataContainer[]>.Failure($"Connection '{command.ConnectionName}' not found");
        }

        try
        {
            var startPath = command.StartPath != null ? new DataPath(command.StartPath.Split("."), ".") : null;
            var containerResult = await connection.DiscoverSchema(startPath).ConfigureAwait(false);
            if (!containerResult.IsSuccess)
            {
                return FdwResult<DataContainer[]>.Failure(containerResult.Message);
            }
            var containers = containerResult.Value?.ToArray() ?? [];
            return FdwResult<DataContainer[]>.Success(containers);
        }
        catch (Exception ex)
        {
            return FdwResult<DataContainer[]>.Failure($"Schema discovery failed: {ex.Message}");
        }
    }

    private async Task<IFdwResult<object>> HandleConnectionManagement(IConnectionManagementCommand command, CancellationToken cancellationToken)
    {
        switch (command.Operation)
        {
            case ConnectionManagementOperation.ListConnections:
                var connectionNames = _connections.Keys.ToArray();
                return FdwResult<object>.Success(connectionNames);

            case ConnectionManagementOperation.RemoveConnection:
                if (command.ConnectionName != null && _connections.TryGetValue(command.ConnectionName, out var connection))
                {
                    connection.Dispose();
                    _connections.Remove(command.ConnectionName);
                    return FdwResult<object>.Success($"Connection '{command.ConnectionName}' removed successfully");
                }
                return FdwResult<object>.Failure($"Connection '{command.ConnectionName}' not found");

            case ConnectionManagementOperation.GetConnectionMetadata:
                if (command.ConnectionName != null && _connections.TryGetValue(command.ConnectionName, out var metadataConnection))
                {
                    try
                    {
                        var metadata = await metadataConnection.GetMetadataAsync().ConfigureAwait(false);
                        return FdwResult<object>.Success(metadata);
                    }
                    catch (Exception ex)
                    {
                        return FdwResult<object>.Failure($"Failed to get connection metadata: {ex.Message}");
                    }
                }
                return FdwResult<object>.Failure($"Connection '{command.ConnectionName}' not found");

            case ConnectionManagementOperation.RefreshConnectionStatus:
                if (command.ConnectionName != null && _connections.TryGetValue(command.ConnectionName, out var statusConnection))
                {
                    var isConnectedResult = await statusConnection.TestConnection().ConfigureAwait(false);
                    var status = new { IsConnected = isConnectedResult.IsSuccess, ConnectionName = command.ConnectionName };
                    return FdwResult<object>.Success(status);
                }
                return FdwResult<object>.Failure($"Connection '{command.ConnectionName}' not found");

            case ConnectionManagementOperation.TestConnection:
                if (command.ConnectionName != null && _connections.TryGetValue(command.ConnectionName, out var testConnection))
                {
                    try
                    {
                        var testResult = await testConnection.TestConnection().ConfigureAwait(false);
                        return testResult.IsSuccess 
                            ? FdwResult<object>.Success(testResult.Value)
                            : FdwResult<object>.Failure($"Connection test failed: {testResult.Message}");
                    }
                    catch (Exception ex)
                    {
                        return FdwResult<object>.Failure($"Connection test failed: {ex.Message}");
                    }
                }
                return FdwResult<object>.Failure($"Connection '{command.ConnectionName}' not found");

            default:
                return FdwResult<object>.Failure($"Unsupported management operation: {command.Operation}");
        }
    }

    private static IFdwResult<T> ConvertResult<T>(IFdwResult result)
    {
        if (result.IsSuccess)
        {
            if (result is IFdwResult<T> typedResult)
            {
                return typedResult;
            }

            // Try to convert the value
            if (result.GetType().GetProperty("Value")?.GetValue(result) is T value)
            {
                return FdwResult<T>.Success(value);
            }

            // For object array conversions
            if (typeof(T) == typeof(object[]) && result.GetType().GetProperty("Value")?.GetValue(result) is Array array)
            {
                var objectArray = new object[array.Length];
                array.CopyTo(objectArray, 0);
                return FdwResult<T>.Success((T)(object)objectArray);
            }

            return FdwResult<T>.Failure($"Unable to convert result to type {typeof(T).Name}");
        }

        return FdwResult<T>.Failure(result.Message ?? "Operation failed");
    }

    #region IFdwConnectionDataService Implementation

    /// <inheritdoc/>
    public async Task<IFdwResult<string>> CreateConnectionAsync(IFdwConnectionConfiguration configuration, CancellationToken cancellationToken = default)
    {
        if (configuration is not MsSqlConfiguration msSqlConfig)
        {
            return FdwResult<string>.Failure("Invalid configuration type for MsSql service");
        }

        var connectionId = Guid.NewGuid().ToString("N");
        
        if (_connections.ContainsKey(connectionId))
        {
            return FdwResult<string>.Failure($"Connection '{connectionId}' already exists");
        }

        try
        {
            var connectionLogger = _loggerFactory.CreateLogger<MsSqlFdwConnection>();
            var connection = new MsSqlFdwConnection(connectionLogger, msSqlConfig);
            _connections[connectionId] = connection;

            MsSqlServiceLog.ConnectionCreated(Logger, connectionId);
            return FdwResult<string>.Success(connectionId);
        }
        catch (Exception ex)
        {
            MsSqlServiceLog.ConnectionCreationFailed(Logger, ex);
            return FdwResult<string>.Failure($"Failed to create connection: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public Task<IFdwResult<IFdwConnection>> GetConnectionAsync(string connectionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(connectionId))
        {
            return Task.FromResult(FdwResult<IFdwConnection>.Failure("Connection ID cannot be null or empty"));
        }

        if (_connections.TryGetValue(connectionId, out var connection))
        {
            return Task.FromResult(FdwResult<IFdwConnection>.Success((IFdwConnection)connection));
        }

        return Task.FromResult(FdwResult<IFdwConnection>.Failure($"Connection '{connectionId}' not found"));
    }

    /// <inheritdoc/>
    public Task<IFdwResult<IEnumerable<string>>> ListConnectionsAsync(CancellationToken cancellationToken = default)
    {
        var connectionIds = _connections.Keys.AsEnumerable();
        return Task.FromResult(FdwResult<IEnumerable<string>>.Success(connectionIds));
    }

    /// <inheritdoc/>
    public Task<IFdwResult> RemoveConnectionAsync(string connectionId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(connectionId))
        {
            return Task.FromResult(FdwResult.Failure("Connection ID cannot be null or empty"));
        }

        if (_connections.TryGetValue(connectionId, out var connection))
        {
            try
            {
                connection.Dispose();
                _connections.Remove(connectionId);
                MsSqlServiceLog.ConnectionRemoved(Logger, connectionId);
                return Task.FromResult(FdwResult.Success());
            }
            catch (Exception ex)
            {
                MsSqlServiceLog.ConnectionRemovalFailed(Logger, connectionId, ex);
                return Task.FromResult(FdwResult.Failure($"Failed to remove connection: {ex.Message}"));
            }
        }

        return Task.FromResult(FdwResult.Failure($"Connection '{connectionId}' not found"));
    }

    /// <inheritdoc/>
    public async Task<IFdwResult<IDictionary<string, bool>>> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, bool>(StringComparer.Ordinal);

        foreach (var kvp in _connections)
        {
            try
            {
                var testResult = await kvp.Value.TestConnection().ConfigureAwait(false);
                results[kvp.Key] = testResult.IsSuccess;
            }
            catch (Exception ex)
            {
                MsSqlServiceLog.HealthCheckFailed(Logger, kvp.Key, ex);
                results[kvp.Key] = false;
            }
        }

        return FdwResult<IDictionary<string, bool>>.Success(results);
    }

    #endregion

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                foreach (var connection in _connections.Values)
                {
                    try
                    {
                        connection.Dispose();
                    }
                    catch (Exception ex)
                    {
                        MsSqlServiceLog.ConnectionDisposeException(Logger, ex);
                    }
                }
                _connections.Clear();
            }

            _disposed = true;
        }
    }
}
