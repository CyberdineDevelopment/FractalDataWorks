using Microsoft.Extensions.Logging;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Authentication.Abstractions;

namespace FractalDataWorks.Services.Authentication;

/// <summary>
/// Base class for authentication service implementations.
/// Provides common authentication service functionality following the standard service pattern.
/// </summary>
/// <typeparam name="TCommand">The command type this authentication service executes.</typeparam>
/// <typeparam name="TConfiguration">The configuration type for the authentication service.</typeparam>
/// <typeparam name="TService">The concrete service type for logging and identification purposes.</typeparam>
/// <remarks>
/// This class provides a foundation for building authentication services that integrate
/// with the FractalDataWorks framework's service management and authentication abstractions.
/// All authentication services should inherit from this class to ensure consistent
/// behavior across different authentication providers (Azure Entra, Local, JWT, etc.).
/// </remarks>
public abstract class AuthenticationServiceBase<TCommand, TConfiguration, TService>
    : ServiceBase<TCommand, TConfiguration, TService>
    where TCommand : IAuthenticationCommand
    where TConfiguration : class, IAuthenticationConfiguration
    where TService : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationServiceBase{TCommand, TConfiguration, TService}"/> class.
    /// </summary>
    /// <param name="logger">The logger for this authentication service.</param>
    /// <param name="configuration">The configuration for this authentication service.</param>
    protected AuthenticationServiceBase(ILogger<TService> logger, TConfiguration configuration)
        : base(logger, configuration)
    {
    }
}