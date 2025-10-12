using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Data.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.Services.DataGateway.Abstractions;

namespace FractalDataWorks.Services.DataGateway;

/// <summary>
/// Default implementation of the DataGateway service.
/// Routes commands to the appropriate connection based on ConnectionName.
/// </summary>
public sealed class DataGatewayService : IDataGateway
{
    private readonly ILogger<DataGatewayService> _logger;
    private readonly IGenericConnectionProvider _connectionProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataGatewayService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="connectionProvider">The connection provider for routing.</param>
    public DataGatewayService(
        ILogger<DataGatewayService> logger,
        IGenericConnectionProvider connectionProvider)
    {
        _logger = logger;
        _connectionProvider = connectionProvider;
    }

    /// <inheritdoc/>
    public async Task<IGenericResult<T>> Execute<T>(IDataCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Routing data command {CommandType} to connection {ConnectionName}",
            command.CommandType, command.ConnectionName);

        // Get connection by name
        var connectionResult = await _connectionProvider.GetConnectionAsync(command.ConnectionName, cancellationToken);
        if (!connectionResult.IsSuccess)
        {
            _logger.LogError("Failed to get connection {ConnectionName}", command.ConnectionName);
            return GenericResult<T>.Failure($"Connection '{command.ConnectionName}' not found");
        }

        var connection = connectionResult.Value;

        // Execute command on the connection
        // Note: Connection service will handle translation (LINQ -> SQL, etc.)
        return await connection.Execute<T>(command, cancellationToken);
    }
}
