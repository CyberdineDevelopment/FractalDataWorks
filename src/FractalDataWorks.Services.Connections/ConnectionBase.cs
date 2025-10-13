using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Abstractions;
using FractalDataWorks.Results;
using FractalDataWorks.Services.Abstractions;
using FractalDataWorks.Services.Connections.Abstractions;

namespace FractalDataWorks.Services.Connections;

/// <summary>
/// Abstract base class for all connection service implementations.
/// Provides common connection service functionality following the standard service pattern.
/// </summary>
/// <typeparam name="TCommand">The command type this connection service executes.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the connection service.</typeparam>
/// <typeparam name="TService">The concrete service type for logging and identification purposes.</typeparam>
/// <remarks>
/// This class provides a foundation for building connection services that integrate
/// with the FractalDataWorks framework's service management and connection abstractions.
/// All connection services should inherit from this class to ensure consistent
/// behavior across different connection types (HTTP, SQL, REST, etc.).
/// </remarks>
public abstract class ConnectionBase<TCommand, TConfiguration, TService> : ServiceBase<TCommand,TConfiguration,TService>, IGenericService<TCommand, TConfiguration, TService>, IGenericConnection
    where TCommand : IConnectionCommand
    where TConfiguration : class, IConnectionConfiguration
    where TService : class
{
    private IConnectionState _state;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionBase{TCommand,TConfiguration,TService}"/> class.
    /// </summary>
    /// <param name="logger">The logger for this connection service.</param>
    /// <param name="configuration">The configuration for this connection service.</param>
    protected ConnectionBase(ILogger<ConnectionBase<TCommand,TConfiguration,TService>> logger, TConfiguration configuration) : base(logger,configuration)
    {
        _state = ConnectionStates.Created;
    }
}