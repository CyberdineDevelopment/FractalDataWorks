using System.Threading.Tasks;
using FractalDataWorks.Results;
using FractalDataWorks.Services.DomainName.Abstractions.Configuration;

namespace FractalDataWorks.Services.DomainName.Abstractions.Providers;

/// <summary>
/// Provider interface for creating and resolving DomainName services.
/// Follows the ServiceType pattern for auto-discovery and service resolution.
/// </summary>
public interface IDomainNameProvider
{
    /// <summary>
    /// Gets a DomainName service using the provided configuration.
    /// The configuration's DomainNameType property determines which factory to use.
    /// </summary>
    /// <param name="configuration">The configuration containing the DomainName type and settings.</param>
    /// <returns>A result containing the DomainName service instance or failure information.</returns>
    Task<IGenericResult<IDomainNameService>> GetDomainNameService(IDomainNameConfiguration configuration);

    /// <summary>
    /// Gets a DomainName service by configuration name from appsettings.
    /// Loads the configuration from the "Services:DomainName:{configurationName}" section.
    /// </summary>
    /// <param name="configurationName">The name of the configuration section.</param>
    /// <returns>A result containing the DomainName service instance or failure information.</returns>
    Task<IGenericResult<IDomainNameService>> GetDomainNameService(string configurationName);
}
