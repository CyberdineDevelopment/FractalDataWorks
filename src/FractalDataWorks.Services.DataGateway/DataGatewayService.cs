using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Results;
using FractalDataWorks.Services;
using FractalDataWorks.Services.DataGateway.Abstractions;

namespace FractalDataWorks.Services.DataGateway;

/// <summary>
/// Default implementation of the DataGateway service.
/// Routes commands to the appropriate connection based on ConnectionName.
/// </summary>
public sealed class DataGatewayService : ServiceBase<IDataGatewayCommand, IDataGatewayConfiguration, DataGatewayService>, IDataGateway
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataGatewayService"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="configuration">The configuration.</param>
    public DataGatewayService(ILogger<DataGatewayService> logger, IDataGatewayConfiguration configuration)
        : base(logger, configuration)
    {
    }

    /// <inheritdoc/>
    public override Task<IGenericResult<T>> Execute<T>(IDataGatewayCommand command, CancellationToken cancellationToken)
    {
        // TODO: Look up connection by command.ConnectionName
        // TODO: Pass command to that connection
        // TODO: Return result
        throw new NotImplementedException("DataGateway routing not yet implemented");
    }

    /// <inheritdoc/>
    public override Task<IGenericResult<T>> Execute<T>(IDataGatewayCommand command)
    {
        return Execute<T>(command, CancellationToken.None);
    }

    /// <inheritdoc/>
    public override Task<IGenericResult> Execute(IDataGatewayCommand command, CancellationToken cancellationToken)
    {
        return Execute<object>(command, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task<IGenericResult> Execute(IDataGatewayCommand command)
    {
        return Execute(command, CancellationToken.None);
    }
}
