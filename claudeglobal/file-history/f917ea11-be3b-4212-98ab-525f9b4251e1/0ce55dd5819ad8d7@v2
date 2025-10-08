using Microsoft.Extensions.Logging;
using FractalDataWorks.Services;
using FractalDataWorks.Services.DomainName.Abstractions.Commands;
using FractalDataWorks.Services.DomainName.Abstractions.Configuration;

namespace FractalDataWorks.Services.DomainName.Abstractions;

/// <summary>
/// Base class for DomainName service implementations.
/// Provides common DomainName service functionality following the standard service pattern.
/// </summary>
/// <typeparam name="TCommand">The command type this DomainName service executes.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the DomainName service.</typeparam>
/// <typeparam name="TService">The concrete service type for logging and identification purposes.</typeparam>
/// <remarks>
/// This class provides a foundation for building DomainName services that integrate
/// with the FractalDataWorks framework's service management and DomainName abstractions.
/// All DomainName services should inherit from this class to ensure consistent
/// behavior across different DomainName providers.
/// </remarks>
public abstract class DomainNameServiceBase<TCommand, TConfiguration, TService>
    : ServiceBase<TCommand, TConfiguration, TService>
    where TCommand : IDomainNameCommand
    where TConfiguration : class, IDomainNameConfiguration
    where TService : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DomainNameServiceBase{TCommand, TConfiguration, TService}"/> class.
    /// </summary>
    /// <param name="logger">The logger for this DomainName service.</param>
    /// <param name="configuration">The configuration for this DomainName service.</param>
    protected DomainNameServiceBase(ILogger<TService> logger, TConfiguration configuration)
        : base(logger, configuration)
    {
    }

    /// <summary>
    /// Gets the service type identifier.
    /// </summary>
    public override string ServiceType => Configuration.DomainNameType;

    // TODO: Add domain-specific abstract methods here
    // Example:
    // public abstract Task<IGenericResult<TResult>> DoSomething(DoSomethingCommand command);
}
