using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Data.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Connections.Abstractions;
using FractalDataWorks.Services.Data.Abstractions;
using FractalDataWorks.Services.Data.Abstractions.Messages;
using FractalDataWorks.Services.Data.Logging;

namespace FractalDataWorks.Services.DataGateway;

/// <summary>
/// Default implementation of the DataGateway service.
/// Routes commands to the appropriate connection based on ConnectionName.
/// </summary>
public sealed class DataGatewayService : IDataGateway
{
    private readonly ILogger<DataGatewayService> _logger;
    private readonly IDataConnectionProvider _connectionProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataGatewayService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="connectionProvider">The data connection provider for routing.</param>
    public DataGatewayService(
        ILogger<DataGatewayService> logger,
        IDataConnectionProvider connectionProvider)
    {
        _logger = logger;
        _connectionProvider = connectionProvider;
    }

    /// <inheritdoc/>
    public async Task<IGenericResult<T>> Execute<T>(IDataCommand command, CancellationToken cancellationToken = default)
    {
        DataGatewayLog.RoutingCommand(_logger, command.CommandType, command.ConnectionName);

        // Get data connection by name
        var connectionResult = await _connectionProvider.GetConnection(command.ConnectionName).ConfigureAwait(false);
        if (!connectionResult.IsSuccess || connectionResult.Value == null)
        {
            DataGatewayLog.ConnectionRetrievalFailed(_logger, command.ConnectionName);
            return GenericResult<T>.Failure(string.Format(CultureInfo.InvariantCulture, DataGatewayMessages.ConnectionNotFound().Message, command.ConnectionName));
        }

        var connection = connectionResult.Value;

        // Execute data command on the data connection
        // Connection service will handle translation (LINQ -> SQL, etc.)
        return await connection.Execute<T>(command, cancellationToken).ConfigureAwait(false);
    }
}
