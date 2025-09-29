using Microsoft.Extensions.Logging;
using FractalDataWorks.Services;
using FractalDataWorks.Services.DataGateway.Abstractions;

namespace FractalDataWorks.Services.DataGateway;

/// <summary>
/// Base class for data gateway service implementations.
/// Provides common data gateway service functionality following the standard service pattern.
/// </summary>
/// <typeparam name="TCommand">The command type this data gateway service executes.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the data gateway service.</typeparam>
/// <typeparam name="TService">The concrete service type for logging and identification purposes.</typeparam>
/// <remarks>
/// This class provides a foundation for building data gateway services that integrate
/// with the FractalDataWorks framework's service management and data gateway abstractions.
/// All data gateway services should inherit from this class to ensure consistent
/// behavior across different data gateway providers.
/// </remarks>
public abstract class DataGatewayServiceBase<TCommand, TConfiguration, TService>
    : ServiceBase<TCommand, TConfiguration, TService>
    where TCommand : IDataGatewayCommand
    where TConfiguration : class, IDataGatewayConfiguration
    where TService : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataGatewayServiceBase{TCommand, TConfiguration, TService}"/> class.
    /// </summary>
    /// <param name="logger">The logger for this data gateway service.</param>
    /// <param name="configuration">The configuration for this data gateway service.</param>
    protected DataGatewayServiceBase(ILogger<TService> logger, TConfiguration configuration)
        : base(logger, configuration)
    {
    }
}