using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Security;

namespace FractalDataWorks.Services.Authentication.AzureEntra;

/// <summary>
/// Factory for creating Azure Entra authentication service instances.
/// Extends GenericServiceFactory to provide Azure Entra-specific service creation.
/// </summary>
public sealed class EntraAuthenticationServiceFactory :
    GenericServiceFactory<IAuthenticationService, IAuthenticationConfiguration>,
    IEntraAuthenticationServiceFactory
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EntraAuthenticationServiceFactory"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public EntraAuthenticationServiceFactory(ILogger<GenericServiceFactory<IAuthenticationService, IAuthenticationConfiguration>> logger)
        : base(logger)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EntraAuthenticationServiceFactory"/> class with no logger.
    /// </summary>
    public EntraAuthenticationServiceFactory()
        : base()
    {
    }
}
