using System;
using Microsoft.Extensions.Logging;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Auth0.Configuration;

namespace FractalDataWorks.Services.Authentication.Auth0;

/// <summary>
/// Factory for creating Auth0 authentication service instances.
/// </summary>
public sealed class Auth0AuthenticationServiceFactory :
    IAuthenticationServiceFactory<IAuthenticationService, IAuthenticationConfiguration>,
    IAuth0AuthenticationServiceFactory
{
    private readonly ILogger<Auth0AuthenticationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="Auth0AuthenticationServiceFactory"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public Auth0AuthenticationServiceFactory(ILogger<Auth0AuthenticationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public IAuthenticationService Create(IAuthenticationConfiguration configuration)
    {
        if (configuration is not Auth0Configuration auth0Config)
        {
            throw new ArgumentException(
                $"Configuration must be of type {nameof(Auth0Configuration)}",
                nameof(configuration));
        }

        return new Auth0AuthenticationService(_logger, auth0Config);
    }
}
