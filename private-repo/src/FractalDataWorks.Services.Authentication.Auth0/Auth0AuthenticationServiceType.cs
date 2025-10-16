using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FractalDataWorks.EnhancedEnums;
using FractalDataWorks.ServiceTypes.Attributes;
using FractalDataWorks.Services;
using FractalDataWorks.Services.Authentication;
using FractalDataWorks.Services.Authentication.Abstractions;
using FractalDataWorks.Services.Authentication.Abstractions.Methods;
using FractalDataWorks.Services.Authentication.Abstractions.Security;
using FractalDataWorks.Services.Authentication.Auth0.Configuration;

namespace FractalDataWorks.Services.Authentication.Auth0;

/// <summary>
/// Service type definition for Auth0 authentication.
/// </summary>
[ServiceTypeOption(typeof(AuthenticationTypes), "Auth0Service")]
public sealed class Auth0AuthenticationServiceType :
    AuthenticationTypeBase<IAuthenticationService, IAuthenticationServiceFactory<IAuthenticationService, IAuthenticationConfiguration>, IAuthenticationConfiguration>,
    IEnumOption<Auth0AuthenticationServiceType>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Auth0AuthenticationServiceType"/> class.
    /// </summary>
    public Auth0AuthenticationServiceType()
        : base(
            id: 1,
            name: "Auth0",
            providerName: "Auth0",
            method: new OAuth2AuthenticationMethod(),
            supportedProtocols: [
                new OAuth2Protocol(),
                new OpenIDConnectProtocol(),
                new SAML2Protocol()
            ],
            supportedFlows: [
                new AuthorizationCodeFlow(),
                new ClientCredentialsFlow(),
                new InteractiveFlow()
            ],
            supportedTokenTypes: [
                new AccessToken(),
                new IdToken(),
                new RefreshToken()
            ],
            supportsMultiTenant: true,
            supportsTokenCaching: true,
            supportsTokenRefresh: true,
            category: "Authentication")
    {
    }

    /// <inheritdoc/>
    public override int Priority => 90;

    /// <inheritdoc/>
    public override void Register(IServiceCollection services)
    {
        // Register the Auth0 authentication service factory
        services.AddScoped<IAuthenticationServiceFactory<IAuthenticationService, IAuthenticationConfiguration>, Auth0AuthenticationServiceFactory>();

        // Register the Auth0 authentication service
        services.AddScoped<IAuthenticationService, Auth0AuthenticationService>();
    }

    /// <inheritdoc/>
    public override void Configure(IConfiguration configuration)
    {
        // Configuration validation can be added here if needed
        // The configuration section is: Services:Authentication:Auth0
    }
}
