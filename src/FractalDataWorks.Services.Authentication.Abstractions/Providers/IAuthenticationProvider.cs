using System.Threading.Tasks;
using FractalDataWorks.Results;

using FractalDataWorks.Services.Authentication.Abstractions.Security;

namespace FractalDataWorks.Services.Authentication.Abstractions;

/// <summary>
/// Provider interface for creating and resolving authentication services.
/// Follows the ServiceType pattern for auto-discovery and service resolution.
/// </summary>
public interface IAuthenticationProvider
{
    /// <summary>
    /// Gets an authentication service using the provided configuration.
    /// The configuration's AuthenticationType property determines which factory to use.
    /// </summary>
    /// <param name="configuration">The configuration containing the authentication type and settings.</param>
    /// <returns>A result containing the authentication service instance or failure information.</returns>
    Task<IGenericResult<IAuthenticationService>> GetAuthenticationService(IAuthenticationConfiguration configuration);

    /// <summary>
    /// Gets an authentication service by configuration name from appsettings.
    /// Loads the configuration from the "Authentication:{configurationName}" section.
    /// </summary>
    /// <param name="configurationName">The name of the configuration section.</param>
    /// <returns>A result containing the authentication service instance or failure information.</returns>
    Task<IGenericResult<IAuthenticationService>> GetAuthenticationService(string configurationName);
}